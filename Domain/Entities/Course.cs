using Domain.Common;

namespace Domain.Entities;

public class Course : BaseEntity
{
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;

    public ICollection<CourseOffering> CourseOfferings { get; set; } = [];
}
