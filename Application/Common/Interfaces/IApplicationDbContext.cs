using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Course> Courses { get; }
    DbSet<Term> Terms { get; }
    DbSet<CourseOffering> CourseOfferings { get; }
    DbSet<CourseOfferingStaff> CourseOfferingStaffs { get; }
    DbSet<Section> Sections { get; }
    DbSet<Module> Modules { get; }
    DbSet<Enrollment> Enrollments { get; }
    DbSet<Session> Sessions { get; }
    DbSet<Attendance> Attendances { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
