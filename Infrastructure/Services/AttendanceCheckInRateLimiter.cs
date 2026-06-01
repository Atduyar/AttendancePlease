using Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services;

public class AttendanceCheckInRateLimiter : IAttendanceCheckInRateLimiter
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(10);

    private readonly IMemoryCache _cache;
    private readonly Lock _lock = new();

    public AttendanceCheckInRateLimiter(IMemoryCache cache)
    {
        _cache = cache;
    }

    public bool TryConsume(int userId, out TimeSpan retryAfter)
    {
        var now = DateTime.UtcNow;
        var cacheKey = $"gps-attendance-checkin:{userId}";

        lock (_lock)
        {
            var attempts = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Window;
                return new List<DateTime>();
            })!;

            attempts.RemoveAll(timestamp => timestamp <= now - Window);

            if (attempts.Count >= MaxAttempts)
            {
                retryAfter = attempts[0].Add(Window) - now;
                return false;
            }

            attempts.Add(now);
            _cache.Set(cacheKey, attempts, Window);
            retryAfter = TimeSpan.Zero;
            return true;
        }
    }
}
