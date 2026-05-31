using System.Security.Cryptography;
using System.Text;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Identity;

public class AttendanceScanTokenService : IAttendanceScanTokenService
{
    private const string Version = "v1";
    private readonly IConfiguration _configuration;

    public AttendanceScanTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AttendanceScanTokenIssueResult Issue(int sessionId)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(GetScanTokenMinutes());
        var expiresUnix = new DateTimeOffset(expiresAt).ToUnixTimeSeconds();
        var nonce = RandomNumberGenerator.GetHexString(6).ToLowerInvariant();
        var payload = $"{Version}.{sessionId}.{expiresUnix}.{nonce}";
        var signature = Base64UrlEncode(Sign(payload));

        return new AttendanceScanTokenIssueResult($"{payload}.{signature}", expiresAt);
    }

    public AttendanceScanTokenValidationResult Validate(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return new AttendanceScanTokenValidationResult(false, null, "Missing scan token.");

        var parts = token.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 5 || parts[0] != Version)
            return new AttendanceScanTokenValidationResult(false, null, "Invalid scan token.");

        if (!int.TryParse(parts[1], out var sessionId) || sessionId <= 0)
            return new AttendanceScanTokenValidationResult(false, null, "Invalid scan token.");

        if (!long.TryParse(parts[2], out var expiresUnix))
            return new AttendanceScanTokenValidationResult(false, null, "Invalid scan token.");

        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresUnix);
        if (DateTimeOffset.UtcNow > expiresAt.AddSeconds(30))
            return new AttendanceScanTokenValidationResult(false, null, "This QR code has expired. Ask your instructor to refresh it.");

        var payload = string.Join('.', parts.Take(4));
        var expected = Base64UrlEncode(Sign(payload));
        if (!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(expected), Encoding.ASCII.GetBytes(parts[4])))
            return new AttendanceScanTokenValidationResult(false, null, "Invalid scan token.");

        return new AttendanceScanTokenValidationResult(true, sessionId, null);
    }

    private byte[] Sign(string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))[..16];
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private int GetScanTokenMinutes()
    {
        return int.TryParse(_configuration["AttendanceScan:TokenMinutes"], out var minutes) ? minutes : 5;
    }
}
