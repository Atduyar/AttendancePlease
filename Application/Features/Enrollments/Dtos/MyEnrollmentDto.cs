namespace Application.Features.Enrollments.Dtos;

public record MyEnrollmentDto(
    int Id,
    int UserId,
    string UserName,
    int CourseOfferingId,
    int CourseId,
    string CourseCode,
    string CourseTitle,
    int TermId,
    string TermCode,
    int SectionId,
    string SectionName,
    DateTime CreatedAt);
