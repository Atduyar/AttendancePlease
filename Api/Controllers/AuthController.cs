using System.Security.Claims;
using Api.Models.Auth;
using Application.Common.Interfaces;
using Application.Features.Auth.Commands;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers;

[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly EntraIdTokenValidator _tokenValidator;
    private readonly EntraIdUserProvisioner _userProvisioner;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        EntraIdTokenValidator tokenValidator,
        EntraIdUserProvisioner userProvisioner,
        IJwtTokenService jwtTokenService,
        UserManager<User> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        IWebHostEnvironment environment,
        ILogger<AuthController> logger)
    {
        _tokenValidator = tokenValidator;
        _userProvisioner = userProvisioner;
        _jwtTokenService = jwtTokenService;
        _userManager = userManager;
        _roleManager = roleManager;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Exchange a Microsoft Entra ID token for our API access + stateless refresh tokens.
    /// The frontend sends the Entra token here, we validate it, provision the user,
    /// and return our own tokens with local DB roles.
    /// </summary>
    [HttpPost("exchange")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthTokenResponse>> Exchange(CancellationToken cancellationToken)
    {
        var authorizationHeader = HttpContext.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Problem(
                title: "Missing authorization header",
                statusCode: StatusCodes.Status401Unauthorized,
                detail: "Expected Bearer token from Microsoft Entra ID.");
        }

        var entraToken = authorizationHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrEmpty(entraToken))
        {
            return Problem(
                title: "Empty token",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        try
        {
            var (principal, entraClaims) = await _tokenValidator.ValidateAndExtractAsync(entraToken, cancellationToken);
            var result = await _userProvisioner.ProvisionAsync(principal, entraClaims, cancellationToken);

            _logger.LogInformation("Token exchange successful for user {Email}", result.Email);

            return Ok(new AuthTokenResponse(
                result.AccessToken,
                result.RefreshToken,
                result.AccessTokenExpiresAt,
                result.RefreshTokenExpiresAt,
                new AuthUserResponse(
                    result.UserId,
                    result.Email,
                    result.Name,
                    result.PrimaryRole,
                    result.Roles)));
        }
        catch (SecurityTokenValidationException ex)
        {
            _logger.LogWarning("Entra token validation failed: {Message}", ex.Message);
            return Problem(
                title: "Invalid token",
                statusCode: StatusCodes.Status401Unauthorized,
                detail: "The provided Microsoft token could not be validated.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("User provisioning failed: {Message}", ex.Message);
            return Problem(
                title: "User provisioning failed",
                statusCode: StatusCodes.Status400BadRequest,
                detail: ex.Message);
        }
    }

    /// <summary>
    /// Use our stateless refresh token to get a new access token and refresh token.
    /// Refresh tokens are not stored/revoked yet; they are valid until expiry.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthTokenResponse>> Refresh(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Problem(
                title: "Missing refresh token",
                statusCode: StatusCodes.Status400BadRequest);
        }

        try
        {
            var principal = _jwtTokenService.ValidateRefreshToken(request.RefreshToken);
            var userIdClaim = principal.FindFirst("sub")?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Problem(
                    title: "Invalid refresh token",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return Problem(
                    title: "User not found",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Count == 0)
            {
                roles = [user.Role.ToString()];
            }

            var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email!, user.Name, roles);
            var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

            var primaryRole = roles.FirstOrDefault() ?? user.Role.ToString();

            return Ok(new AuthTokenResponse(
                accessToken.Token,
                refreshToken.Token,
                accessToken.ExpiresAt,
                refreshToken.ExpiresAt,
                new AuthUserResponse(
                    user.Id,
                    user.Email!,
                    user.Name,
                    primaryRole,
                    roles.ToList())));
        }
        catch (SecurityTokenValidationException ex)
        {
            _logger.LogWarning("Refresh token validation failed: {Message}", ex.Message);
            return Problem(
                title: "Invalid refresh token",
                statusCode: StatusCodes.Status401Unauthorized);
        }
    }

    [HttpPost("dev-role")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthTokenResponse>> DevRole(DevRoleRequest request)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        User? user = null;
        if (request.UserId.HasValue)
        {
            user = await _userManager.FindByIdAsync(request.UserId.Value.ToString());
        }

        if (user == null && !string.IsNullOrWhiteSpace(request.Email))
        {
            user = await _userManager.FindByEmailAsync(request.Email.Trim().ToLowerInvariant());
        }

        if (user == null)
        {
            return Problem(
                title: "User not found",
                statusCode: StatusCodes.Status404NotFound,
                detail: "Provide a valid userId or email.");
        }

        var roleName = request.Role.ToString();
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var createRole = await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
            if (!createRole.Succeeded)
            {
                return Problem(
                    title: "Role creation failed",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?> { ["errors"] = createRole.Errors.Select(e => e.Description).ToArray() });
            }
        }

        var appRoles = Enum.GetNames<UserRole>();
        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.Where(role => appRoles.Contains(role)).ToArray();
        if (rolesToRemove.Length > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                return Problem(
                    title: "Role removal failed",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?> { ["errors"] = removeResult.Errors.Select(e => e.Description).ToArray() });
            }
        }

        var addResult = await _userManager.AddToRoleAsync(user, roleName);
        if (!addResult.Succeeded)
        {
            return Problem(
                title: "Role assignment failed",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?> { ["errors"] = addResult.Errors.Select(e => e.Description).ToArray() });
        }

        user.Role = request.Role;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email!, user.Name, roles);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);
        var primaryRole = roles.FirstOrDefault() ?? roleName;

        return Ok(new AuthTokenResponse(
            accessToken.Token,
            refreshToken.Token,
            accessToken.ExpiresAt,
            refreshToken.ExpiresAt,
            new AuthUserResponse(
                user.Id,
                user.Email!,
                user.Name,
                primaryRole,
                roles.ToList())));
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Register(RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return Problem(
                title: "Registration failed",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?> { ["errors"] = result.Errors });
        }

        return Ok(new
        {
            result.Token,
            result.UserId,
            result.Email,
            result.Name,
            result.Role
        });
    }
}

public record RefreshTokenRequest(string RefreshToken);

public record DevRoleRequest(int? UserId, string? Email, UserRole Role);
