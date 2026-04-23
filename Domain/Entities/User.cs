using Domain.Common;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class User : IdentityUser<int>, IBaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<CourseOfferingStaff> CourseOfferingStaffs { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<Session> OpenedSessions { get; set; } = [];
    public ICollection<Attendance> Attendances { get; set; } = [];
}
