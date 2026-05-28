using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(int userId, string email, string name, IEnumerable<string> roles)
    {
        return GenerateAccessToken(userId, email, name, roles).Token;
    }

    public string GenerateToken(int userId, string email, string name, IEnumerable<string> roles,
        Dictionary<string, object?>? entraClaims)
    {
        return GenerateAccessToken(userId, email, name, roles, entraClaims).Token;
    }

    public JwtTokenResult GenerateAccessToken(int userId, string email, string name, IEnumerable<string> roles,
        Dictionary<string, object?>? entraClaims = null)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenMinutes());
        var claims = new List<Claim>
        {
            new("sub", userId.ToString()),
            new("email", email),
            new("name", name),
            new("jti", Guid.NewGuid().ToString()),
            new("token_type", "access")
        };

        claims.AddRange(roles.Select(role => new Claim("role", role)));

        // Embed Entra ID claims only in access token. Refresh token stays minimal.
        if (entraClaims is { Count: > 0 })
        {
            var entraJson = JsonSerializer.Serialize(entraClaims, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
            claims.Add(new Claim("entra_data", entraJson, JsonClaimValueTypes.Json));
        }

        return new JwtTokenResult(WriteToken(claims, expiresAt), expiresAt);
    }

    public JwtTokenResult GenerateRefreshToken(int userId)
    {
        var expiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenDays());
        var claims = new List<Claim>
        {
            new("sub", userId.ToString()),
            new("jti", Guid.NewGuid().ToString()),
            new("token_type", "refresh")
        };

        return new JwtTokenResult(WriteToken(claims, expiresAt), expiresAt);
    }

    public ClaimsPrincipal ValidateRefreshToken(string refreshToken)
    {
        var principal = ValidateToken(refreshToken);
        var tokenType = principal.FindFirst("token_type")?.Value;

        if (!string.Equals(tokenType, "refresh", StringComparison.Ordinal))
        {
            throw new SecurityTokenValidationException("Token is not a refresh token.");
        }

        return principal;
    }

    private ClaimsPrincipal ValidateToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        return handler.ValidateToken(token, GetValidationParameters(), out _);
    }

    private string WriteToken(IEnumerable<Claim> claims, DateTime expiresAt)
    {
        var creds = new SigningCredentials(GetSigningKey(), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private TokenValidationParameters GetValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],
            IssuerSigningKey = GetSigningKey(),
            ClockSkew = TimeSpan.FromMinutes(5),
            NameClaimType = "sub",
            RoleClaimType = "role"
        };
    }

    private SymmetricSecurityKey GetSigningKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
    }

    private int GetAccessTokenMinutes()
    {
        return int.TryParse(_configuration["Jwt:AccessTokenMinutes"], out var minutes) ? minutes : 60;
    }

    private int GetRefreshTokenDays()
    {
        return int.TryParse(_configuration["Jwt:RefreshTokenDays"], out var days) ? days : 7;
    }
}
