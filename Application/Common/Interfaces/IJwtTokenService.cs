using System.Security.Claims;
using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface IJwtTokenService
{
    // Backward-compatible helper for older local auth code.
    string GenerateToken(int userId, string email, string name, IEnumerable<string> roles);
    string GenerateToken(int userId, string email, string name, IEnumerable<string> roles,
        Dictionary<string, object?>? entraClaims);

    JwtTokenResult GenerateAccessToken(int userId, string email, string name, IEnumerable<string> roles,
        Dictionary<string, object?>? entraClaims = null);

    JwtTokenResult GenerateRefreshToken(int userId);

    ClaimsPrincipal ValidateRefreshToken(string refreshToken);
}
