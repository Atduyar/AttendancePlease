namespace Application.Features.AttendanceSessions.Dtos;

/// <summary>
/// Represents a GPS-verified attendance session created by a lecturer.
/// </summary>
public record AttendanceSessionDto(
    Guid Id,
    int CourseOfferingId,
    string CourseName,
    int CreatedByUserId,
    string SessionToken,
    double Latitude,
    double Longitude,
    double RadiusMeters,
    DateTime ExpiresAt,
    DateTime CreatedAt,
    bool IsActive);
