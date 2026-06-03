using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IAttendancePinService
{
    AttendancePinResult GetCurrentPin(Session session, DateTime? now = null);
    bool ValidatePin(Session session, string? pin, DateTime? now = null);
}

public record AttendancePinResult(
    string Pin,
    int RotationSeconds,
    int SecondsRemaining,
    DateTime ExpiresAt);
