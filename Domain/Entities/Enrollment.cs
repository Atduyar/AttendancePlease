using Domain.Common;

namespace Domain.Entities;

public class Enrollment : BaseEntity
{
    public int? UserId { get; set; }
    public string StudentNumber { get; set; } = null!;
    public string? ImportedName { get; set; }
    public int CourseOfferingId { get; set; }
    public int SectionId { get; set; }

    public User? User { get; set; }
    public CourseOffering CourseOffering { get; set; } = null!;
    public Section Section { get; set; } = null!;
}
