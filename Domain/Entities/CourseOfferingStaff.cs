using Domain.Common;

namespace Domain.Entities;

public class CourseOfferingStaff : BaseEntity
{
    public int CourseOfferingId { get; set; }
    public int UserId { get; set; }
    public string? RoleTitle { get; set; }

    public CourseOffering CourseOffering { get; set; } = null!;
    public User User { get; set; } = null!;
}
