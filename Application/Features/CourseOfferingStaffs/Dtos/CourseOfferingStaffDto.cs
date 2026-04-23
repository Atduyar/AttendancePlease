namespace Application.Features.CourseOfferingStaffs.Dtos;

public record CourseOfferingStaffDto(int Id, int CourseOfferingId, int UserId, string UserName, string? RoleTitle, DateTime CreatedAt);
