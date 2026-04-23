using Application.Common.Exceptions;
using Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Features.CourseOfferings.Commands;

public record DeleteCourseOfferingCommand(int Id) : IRequest;

public class DeleteCourseOfferingCommandValidator : AbstractValidator<DeleteCourseOfferingCommand>
{
    public DeleteCourseOfferingCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeleteCourseOfferingCommandHandler : IRequestHandler<DeleteCourseOfferingCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCourseOfferingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCourseOfferingCommand request, CancellationToken cancellationToken)
    {
        var offering = await _context.CourseOfferings.FindAsync(request.Id, cancellationToken);
        if (offering == null) throw new NotFoundException(nameof(offering), request.Id);

        _context.CourseOfferings.Remove(offering);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
