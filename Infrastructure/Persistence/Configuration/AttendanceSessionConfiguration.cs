using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration;

public class AttendanceSessionConfiguration : IEntityTypeConfiguration<AttendanceSession>
{
    public void Configure(EntityTypeBuilder<AttendanceSession> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SessionToken)
            .HasMaxLength(128);

        builder.HasIndex(x => x.SessionToken)
            .IsUnique();

        builder.HasOne(x => x.CourseOffering)
            .WithMany()
            .HasForeignKey(x => x.CourseOfferingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
