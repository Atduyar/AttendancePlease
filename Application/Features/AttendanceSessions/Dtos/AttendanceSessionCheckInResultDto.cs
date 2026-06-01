namespace Application.Features.AttendanceSessions.Dtos;

/// <summary>
/// Result returned after a student submits GPS coordinates for attendance verification.
/// </summary>
public record AttendanceSessionCheckInResultDto(
    bool Approved,
    double? DistanceMeters,
    string Message);

/// <summary>
/// Payload submitted by a lecturer when opening a GPS attendance session.
/// </summary>
public record CreateAttendanceSessionRequest(
    int CourseOfferingId,
    double Latitude,
    double Longitude,
    double RadiusMeters,
    int DurationMinutes);

/// <summary>
/// GPS coordinates submitted by a student during attendance verification.
/// </summary>
public record AttendanceSessionCheckInRequest(
    double Latitude,
    double Longitude);
