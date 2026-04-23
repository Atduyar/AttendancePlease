using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;

    public IdentityService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Application.Common.Models.IdentityResult> RegisterAsync(
        string name, string email, string password, UserRole role, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            Name = name,
            UserName = email,
            Email = email,
            Role = role
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return new Application.Common.Models.IdentityResult(null, 0, email, name, role.ToString(), result.Errors.Select(e => e.Description).ToArray());

        await _userManager.AddToRoleAsync(user, role.ToString());

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenService.GenerateToken(user.Id, user.Email!, user.Name, roles);
        return new Application.Common.Models.IdentityResult(token, user.Id, user.Email!, user.Name, role.ToString(), Array.Empty<string>());
    }

    public async Task<Application.Common.Models.IdentityResult> LoginAsync(
        string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return new Application.Common.Models.IdentityResult(null, 0, string.Empty, string.Empty, string.Empty, new[] { "Invalid credentials" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded)
            return new Application.Common.Models.IdentityResult(null, 0, string.Empty, string.Empty, string.Empty, new[] { "Invalid credentials" });

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? user.Role.ToString();
        var token = _jwtTokenService.GenerateToken(user.Id, user.Email!, user.Name, roles);
        return new Application.Common.Models.IdentityResult(token, user.Id, user.Email!, user.Name, role, Array.Empty<string>());
    }
}
