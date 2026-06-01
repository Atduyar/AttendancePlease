namespace Application.Features.AttendanceSessions.Dtos;

/// <summary>
/// Public metadata returned to students after scanning a GPS attendance QR code.
/// </summary>
public record AttendanceSessionMetadataDto(
    Guid Id,
    int CourseOfferingId,
    string CourseName,
    DateTime ExpiresAt,
    bool IsActive);
