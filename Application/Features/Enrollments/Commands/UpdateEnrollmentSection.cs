using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Enrollments.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Enrollments.Commands;

public record UpdateEnrollmentSectionCommand(int Id, int SectionId) : IRequest<EnrollmentDto>;

public class UpdateEnrollmentSectionCommandValidator : AbstractValidator<UpdateEnrollmentSectionCommand>
{
    public UpdateEnrollmentSectionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.SectionId).GreaterThan(0);
    }
}

public class UpdateEnrollmentSectionCommandHandler : IRequestHandler<UpdateEnrollmentSectionCommand, EnrollmentDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateEnrollmentSectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EnrollmentDto> Handle(UpdateEnrollmentSectionCommand request, CancellationToken cancellationToken)
    {
        var enrollment = await _context.Enrollments.FindAsync(request.Id, cancellationToken);
        if (enrollment == null) throw new NotFoundException(nameof(enrollment), request.Id);

        enrollment.SectionId = request.SectionId;
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
