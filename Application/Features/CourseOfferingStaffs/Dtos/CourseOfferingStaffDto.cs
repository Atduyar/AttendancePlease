using Domain.Enums;

namespace Application.Features.CourseOfferingStaffs.Dtos;

public record CourseOfferingStaffDto(
    int Id,
    int CourseOfferingId,
    int? SectionId,
    string? SectionName,
    int UserId,
    string UserName,
    string UserEmail,
    string UserRole,
    CourseOfferingStaffScope Scope,
    CourseOfferingStaffAccessLevel AccessLevel,
    string? RoleTitle,
    DateTime CreatedAt);
