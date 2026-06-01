namespace Application.Features.AttendanceSessions.Dtos;

/// <summary>
/// A recorded GPS check-in attempt for a lecturer-managed attendance session.
/// </summary>
public record AttendanceSessionRecordDto(
    Guid Id,
    Guid SessionId,
    int UserId,
    string UserDisplayName,
    double Latitude,
    double Longitude,
    double DistanceMeters,
    bool IsApproved,
    DateTime RecordedAt);
