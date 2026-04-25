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

    public async Task<(int UserId, string[] Errors)> RegisterAsync(
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
            return (0, result.Errors.Select(e => e.Description).ToArray());

        await _userManager.AddToRoleAsync(user, role.ToString());
        return (user.Id, Array.Empty<string>());
    }

    public async Task<(string? Token, string[] Errors)> LoginAsync(
        string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return (null, new[] { "Invalid credentials" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded)
            return (null, new[] { "Invalid credentials" });

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenService.GenerateToken(user.Id, user.Email!, user.Name, roles);
        return (token, Array.Empty<string>());
    }
}
