using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class CourseOfferingStaff : BaseEntity
{
    public int CourseOfferingId { get; set; }
    public int? SectionId { get; set; }
    public int UserId { get; set; }
    public CourseOfferingStaffScope Scope { get; set; } = CourseOfferingStaffScope.Offering;
    public CourseOfferingStaffAccessLevel AccessLevel { get; set; } = CourseOfferingStaffAccessLevel.Assistant;
    public string? RoleTitle { get; set; }

    public CourseOffering CourseOffering { get; set; } = null!;
    public Section? Section { get; set; }
    public User User { get; set; } = null!;
}
