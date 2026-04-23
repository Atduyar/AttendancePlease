using Domain.Common;

namespace Domain.Entities;

public class CourseOffering : BaseEntity
{
    public int CourseId { get; set; }
    public int TermId { get; set; }
    public string? Note { get; set; }

    public Course Course { get; set; } = null!;
    public Term Term { get; set; } = null!;
    public ICollection<CourseOfferingStaff> Staff { get; set; } = [];
    public ICollection<Section> Sections { get; set; } = [];
    public ICollection<Module> Modules { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
}
