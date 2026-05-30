using System.Security.Claims;
using Application.Common.Interfaces;
using Application.Features.Enrollments;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity;

public class EntraIdUserProvisioner
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<EntraIdUserProvisioner> _logger;

    public EntraIdUserProvisioner(
        UserManager<User> userManager,
        IJwtTokenService jwtTokenService,
        IApplicationDbContext context,
        ILogger<EntraIdUserProvisioner> logger)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Finds or creates a local User from Entra ID token claims.
    /// Returns our access + stateless refresh token with local DB roles.
    /// </summary>
    public async Task<ProvisionResult> ProvisionAsync(
        ClaimsPrincipal entraPrincipal,
        Dictionary<string, object?> entraClaims,
        CancellationToken cancellationToken = default)
    {
        var entraOid = entraPrincipal.FindFirst("oid")?.Value
                       ?? entraPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? throw new InvalidOperationException("Entra token missing 'oid' claim.");

        var email = entraPrincipal.FindFirst("preferred_username")?.Value
                    ?? entraPrincipal.FindFirst(ClaimTypes.Email)?.Value
                    ?? entraPrincipal.FindFirst("email")?.Value
                    ?? throw new InvalidOperationException("Entra token missing email claim.");

        email = email.Trim().ToLowerInvariant();

        var displayName = entraPrincipal.FindFirst("name")?.Value
                          ?? entraPrincipal.FindFirst(ClaimTypes.Name)?.Value
                          ?? email;
        var studentNumber = StudentNumber.FromStudentEmail(email);

        var user = await _userManager.FindByLoginAsync("EntraId", entraOid);

        if (user == null)
        {
            user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var loginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo("EntraId", entraOid, "Microsoft Entra ID"));
                if (!loginResult.Succeeded)
                {
                    var errors = loginResult.Errors.Select(e => e.Description).ToArray();
                    throw new InvalidOperationException($"Failed to link Entra login: {string.Join(", ", errors)}");
                }

                _logger.LogInformation("Linked existing user {Email} to Entra ID {Oid}", email, entraOid);
            }
        }

        if (user == null)
        {
            var derivedRole = DeriveRoleFromEmail(email);

            user = new User
            {
                UserName = $"entra_{entraOid}",
                Email = email,
                Name = displayName,
                Role = derivedRole,
                StudentNumber = studentNumber,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToArray();
                throw new InvalidOperationException($"Failed to create user: {string.Join(", ", errors)}");
            }

            var roleResult = await _userManager.AddToRoleAsync(user, derivedRole.ToString());
            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.Select(e => e.Description).ToArray();
                throw new InvalidOperationException($"Failed to assign role: {string.Join(", ", errors)}");
            }

            var loginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo("EntraId", entraOid, "Microsoft Entra ID"));
            if (!loginResult.Succeeded)
            {
                var errors = loginResult.Errors.Select(e => e.Description).ToArray();
                throw new InvalidOperationException($"Failed to add Entra login: {string.Join(", ", errors)}");
            }

            _logger.LogInformation("Created new {Role} user {Email} from Entra ID {Oid}", derivedRole, email, entraOid);
        }
        else
        {
            if (user.Name != displayName || user.Email != email || user.StudentNumber != studentNumber)
            {
                user.Name = displayName;
                user.Email = email;
                if (!string.IsNullOrWhiteSpace(studentNumber)) user.StudentNumber = studentNumber;
                await _userManager.UpdateAsync(user);
            }
        }

        if (!string.IsNullOrWhiteSpace(user.StudentNumber))
        {
            var pendingEnrollments = await _context.Enrollments
                .Where(e => e.UserId == null && e.StudentNumber == user.StudentNumber)
                .ToListAsync(cancellationToken);
            foreach (var enrollment in pendingEnrollments)
            {
                enrollment.UserId = user.Id;
            }

            if (pendingEnrollments.Count > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Linked {Count} pending enrollment(s) to student {StudentNumber}", pendingEnrollments.Count, user.StudentNumber);
            }
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count == 0)
        {
            await _userManager.AddToRoleAsync(user, user.Role.ToString());
            roles = await _userManager.GetRolesAsync(user);
        }

        var primaryRole = roles.FirstOrDefault() ?? user.Role.ToString();
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email!, user.Name, roles, entraClaims);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

        return new ProvisionResult(
            accessToken.Token,
            accessToken.ExpiresAt,
            refreshToken.Token,
            refreshToken.ExpiresAt,
            user.Id,
            user.Email!,
            user.Name,
            primaryRole,
            roles.ToList());
    }

    private static UserRole DeriveRoleFromEmail(string email)
    {
        if (email.EndsWith("@student.ius.edu.ba", StringComparison.OrdinalIgnoreCase))
        {
            return UserRole.Student;
        }

        if (email.EndsWith("@ius.edu.ba", StringComparison.OrdinalIgnoreCase))
        {
            return UserRole.Staff;
        }

        throw new InvalidOperationException("Only IUS student/staff email addresses are allowed.");
    }
}

public record ProvisionResult(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    int UserId,
    string Email,
    string Name,
    string PrimaryRole,
    List<string> Roles);
