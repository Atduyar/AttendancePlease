using Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Features.CourseOfferings.Commands;

public record UpdateCourseOfferingCommand(int Id, string? Note) : IRequest<bool>;

public class UpdateCourseOfferingCommandValidator : AbstractValidator<UpdateCourseOfferingCommand>
{
    public UpdateCourseOfferingCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class UpdateCourseOfferingCommandHandler : IRequestHandler<UpdateCourseOfferingCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateCourseOfferingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateCourseOfferingCommand request, CancellationToken cancellationToken)
    {
        var offering = await _context.CourseOfferings.FindAsync(new object[] { request.Id }, cancellationToken);
        if (offering == null) return false;

        offering.Note = request.Note;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
