namespace Application.Features.Modules.Dtos;

public record ModuleDto(int Id, int CourseOfferingId, string Title, int OrderIndex, DateTime CreatedAt);
