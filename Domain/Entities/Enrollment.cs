using Domain.Common;

namespace Domain.Entities;

public class Enrollment : BaseEntity
{
    public int UserId { get; set; }
    public int CourseOfferingId { get; set; }
    public int SectionId { get; set; }

    public User User { get; set; } = null!;
    public CourseOffering CourseOffering { get; set; } = null!;
    public Section Section { get; set; } = null!;
}
