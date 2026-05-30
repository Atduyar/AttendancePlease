using Application.Features.Enrollments.Dtos;
using Domain.Entities;

namespace Application.Features.Enrollments;

public static class EnrollmentMapping
{
    public static EnrollmentDto ToDto(this Enrollment enrollment)
    {
        return new EnrollmentDto(
            enrollment.Id,
            enrollment.UserId,
            enrollment.StudentNumber,
            enrollment.User?.Name ?? enrollment.ImportedName ?? "Pending student",
            enrollment.CourseOfferingId,
            enrollment.SectionId,
            enrollment.Section.Name,
            enrollment.UserId.HasValue,
            enrollment.CreatedAt);
    }
}
