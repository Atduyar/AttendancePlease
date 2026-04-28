namespace Application.Features.CourseOfferings.Dtos;

public record CourseOfferingDto(int Id, int CourseId, string CourseCode, string CourseTitle, int TermId, string TermCode, string? Note, DateTime CreatedAt);
