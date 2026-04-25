using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public UserRole Role { get; set; }

    public ICollection<CourseOfferingStaff> CourseOfferingStaffs { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<Session> OpenedSessions { get; set; } = [];
    public ICollection<Attendance> Attendances { get; set; } = [];
}
