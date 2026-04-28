using Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Features.Enrollments.Commands;

public record UpdateEnrollmentSectionCommand(int Id, int SectionId) : IRequest<bool>;

public class UpdateEnrollmentSectionCommandValidator : AbstractValidator<UpdateEnrollmentSectionCommand>
{
    public UpdateEnrollmentSectionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.SectionId).GreaterThan(0);
    }
}

public class UpdateEnrollmentSectionCommandHandler : IRequestHandler<UpdateEnrollmentSectionCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateEnrollmentSectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateEnrollmentSectionCommand request, CancellationToken cancellationToken)
    {
        var enrollment = await _context.Enrollments.FindAsync(new object[] { request.Id }, cancellationToken);
        if (enrollment == null) return false;

        enrollment.SectionId = request.SectionId;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
