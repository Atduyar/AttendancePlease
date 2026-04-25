using Domain.Common;

namespace Domain.Entities;

public class Section : BaseEntity
{
    public int CourseOfferingId { get; set; }
    public string Name { get; set; } = null!;

    public CourseOffering CourseOffering { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<Session> Sessions { get; set; } = [];
}
