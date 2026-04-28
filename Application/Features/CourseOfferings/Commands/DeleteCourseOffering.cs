using Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Features.CourseOfferings.Commands;

public record DeleteCourseOfferingCommand(int Id) : IRequest<bool>;

public class DeleteCourseOfferingCommandValidator : AbstractValidator<DeleteCourseOfferingCommand>
{
    public DeleteCourseOfferingCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeleteCourseOfferingCommandHandler : IRequestHandler<DeleteCourseOfferingCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteCourseOfferingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCourseOfferingCommand request, CancellationToken cancellationToken)
    {
        var offering = await _context.CourseOfferings.FindAsync(new object[] { request.Id }, cancellationToken);
        if (offering == null) return false;

        _context.CourseOfferings.Remove(offering);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
