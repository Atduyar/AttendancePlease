using Application.Common.Interfaces;
using Application.Features.Enrollments;
using Application.Features.Enrollments.Dtos;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Enrollments.Commands;

public record EnrollStudentCommand(int? UserId, string? StudentNumber, string? ImportedName, int CourseOfferingId, int SectionId) : IRequest<EnrollmentDto>;

public class EnrollStudentCommandValidator : AbstractValidator<EnrollStudentCommand>
{
    public EnrollStudentCommandValidator()
    {
        RuleFor(x => x.CourseOfferingId).GreaterThan(0);
        RuleFor(x => x.SectionId).GreaterThan(0);
        RuleFor(x => x).Must(x => x.UserId is > 0 || !string.IsNullOrWhiteSpace(x.StudentNumber))
            .WithMessage("Select a registered student or enter a student number.");
    }
}

public class EnrollStudentCommandHandler : IRequestHandler<EnrollStudentCommand, EnrollmentDto>
{
    private readonly IApplicationDbContext _context;

    public EnrollStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EnrollmentDto> Handle(EnrollStudentCommand request, CancellationToken cancellationToken)
    {
        var sectionBelongsToOffering = await _context.Sections.AnyAsync(
            section => section.Id == request.SectionId && section.CourseOfferingId == request.CourseOfferingId,
            cancellationToken);
        if (!sectionBelongsToOffering)
        {
            throw new Application.Common.Exceptions.ValidationException(new Dictionary<string, string[]>
            {
                [nameof(request.SectionId)] = ["Section must belong to the selected course offering."]
            });
        }

        User? user = null;
        if (request.UserId.HasValue)
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId.Value, cancellationToken);
            if (user == null)
            {
                throw new Application.Common.Exceptions.ValidationException(new Dictionary<string, string[]>
                {
                    [nameof(request.UserId)] = ["Selected user does not exist."]
                });
            }
        }

        var studentNumber = !string.IsNullOrWhiteSpace(request.StudentNumber)
            ? StudentNumber.Normalize(request.StudentNumber)
            : user?.StudentNumber ?? StudentNumber.FromStudentEmail(user?.Email);

        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            throw new Application.Common.Exceptions.ValidationException(new Dictionary<string, string[]>
            {
                [nameof(request.StudentNumber)] = ["Student number is required when the selected user does not have one."]
            });
        }

        if (user != null && user.StudentNumber != studentNumber)
        {
            user.StudentNumber = studentNumber;
        }

        var duplicate = await _context.Enrollments.AnyAsync(
            e => e.CourseOfferingId == request.CourseOfferingId && e.StudentNumber == studentNumber,
            cancellationToken);
        if (duplicate)
        {
            throw new Application.Common.Exceptions.ValidationException(new Dictionary<string, string[]>
            {
                [nameof(request.StudentNumber)] = ["This student is already enrolled in this course offering."]
            });
        }

        var enrollment = new Enrollment
        {
            UserId = user?.Id,
            StudentNumber = studentNumber,
            ImportedName = string.IsNullOrWhiteSpace(request.ImportedName) ? null : request.ImportedName.Trim(),
            CourseOfferingId = request.CourseOfferingId,
            SectionId = request.SectionId
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync(cancellationToken);

        var result = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.Section)
            .FirstAsync(e => e.Id == enrollment.Id, cancellationToken);

        return result.ToDto();
    }
}
