namespace Application.Features.Enrollments.Dtos;

public record EnrollmentDto(int Id, int UserId, string UserName, int CourseOfferingId, int SectionId, string SectionName, DateTime CreatedAt);
