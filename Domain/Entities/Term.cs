using Domain.Common;

namespace Domain.Entities;

public class Term : BaseEntity
{
    public string Code { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public ICollection<CourseOffering> CourseOfferings { get; set; } = [];
}
