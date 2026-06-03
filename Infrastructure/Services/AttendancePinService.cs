using System.Security.Cryptography;
using System.Text;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class AttendancePinService : IAttendancePinService
{
    private readonly IConfiguration _configuration;

    public AttendancePinService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AttendancePinResult GetCurrentPin(Session session, DateTime? now = null)
    {
        var utcNow = DateTime.SpecifyKind(now ?? DateTime.UtcNow, DateTimeKind.Utc);
        var rotationSeconds = GetRotationSeconds();
        var unixSeconds = new DateTimeOffset(utcNow).ToUnixTimeSeconds();
        var bucket = unixSeconds / rotationSeconds;
        var elapsedInBucket = (int)(unixSeconds % rotationSeconds);
        var secondsRemaining = rotationSeconds - elapsedInBucket;
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds((bucket + 1) * rotationSeconds).UtcDateTime;

        return new AttendancePinResult(
            CreatePin(session, bucket),
            rotationSeconds,
            secondsRemaining,
            expiresAt);
    }

    public bool ValidatePin(Session session, string? pin, DateTime? now = null)
    {
        if (string.IsNullOrWhiteSpace(pin)) return false;

        var normalizedPin = NormalizePin(pin);
        if (normalizedPin == null) return false;

        var current = GetCurrentPin(session, now).Pin;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(current),
            Encoding.ASCII.GetBytes(normalizedPin));
    }

    private string CreatePin(Session session, long bucket)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
        var payload = $"attendance-pin.v1.{session.Id}.{session.OpenedAt.Ticks}.{bucket}";

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var value = BitConverter.ToUInt32(hash, 0) % 1_000_000;
        return value.ToString("D6");
    }

    private int GetRotationSeconds()
    {
        if (int.TryParse(_configuration["AttendancePin:RotationSeconds"], out var seconds))
        {
            return Math.Clamp(seconds, 5, 300);
        }

        return 10;
    }

    private static string? NormalizePin(string pin)
    {
        var digits = new string(pin.Where(char.IsDigit).ToArray());
        return digits.Length == 6 ? digits : null;
    }
}
