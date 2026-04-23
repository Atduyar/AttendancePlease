using Application.Common.Interfaces;
using Application.Features.Enrollments.Dtos;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Enrollments.Commands;

public record EnrollStudentCommand(int UserId, int CourseOfferingId, int SectionId) : IRequest<EnrollmentDto>;

public class EnrollStudentCommandValidator : AbstractValidator<EnrollStudentCommand>
{
    public EnrollStudentCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.CourseOfferingId).GreaterThan(0);
        RuleFor(x => x.SectionId).GreaterThan(0);
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
        var enrollment = new Enrollment
        {
            UserId = request.UserId,
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

        return new EnrollmentDto(
            result.Id,
            result.UserId,
            result.User.Name,
            result.CourseOfferingId,
            result.SectionId,
            result.Section.Name,
            result.CreatedAt);
    }
}
