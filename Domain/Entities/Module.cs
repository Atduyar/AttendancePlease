using Domain.Common;

namespace Domain.Entities;

public class Module : BaseEntity
{
    public int CourseOfferingId { get; set; }
    public string Title { get; set; } = null!;
    public int OrderIndex { get; set; }

    public CourseOffering CourseOffering { get; set; } = null!;
    public ICollection<Session> Sessions { get; set; } = [];
}
