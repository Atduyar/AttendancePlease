namespace Application.Features.Enrollments.Dtos;

public record EnrollmentDto(
    int Id,
    int? UserId,
    string StudentNumber,
    string UserName,
    int CourseOfferingId,
    int SectionId,
    string SectionName,
    bool IsLinkedUser,
    DateTime CreatedAt);
