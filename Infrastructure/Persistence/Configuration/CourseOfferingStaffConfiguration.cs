using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration;

public class CourseOfferingStaffConfiguration : IEntityTypeConfiguration<CourseOfferingStaff>
{
    public void Configure(EntityTypeBuilder<CourseOfferingStaff> builder)
    {
        builder.Property(s => s.RoleTitle).HasMaxLength(100);
        builder.HasIndex(s => new { s.CourseOfferingId, s.UserId })
            .IsUnique()
            .HasFilter("\"Scope\" = 0");
        builder.HasIndex(s => new { s.CourseOfferingId, s.UserId, s.SectionId })
            .IsUnique()
            .HasFilter("\"Scope\" = 1");

        builder.HasOne(s => s.Section)
            .WithMany(s => s.StaffAssignments)
            .HasForeignKey(s => s.SectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
