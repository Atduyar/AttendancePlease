namespace Application.Common.Interfaces;

public interface IAttendanceCheckInRateLimiter
{
    bool TryConsume(int userId, out TimeSpan retryAfter);
}
