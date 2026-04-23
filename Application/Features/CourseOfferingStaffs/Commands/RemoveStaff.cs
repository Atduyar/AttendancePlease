using Application.Common.Exceptions;
using Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Features.CourseOfferingStaffs.Commands;

public record RemoveStaffCommand(int Id) : IRequest;

public class RemoveStaffCommandValidator : AbstractValidator<RemoveStaffCommand>
{
    public RemoveStaffCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class RemoveStaffCommandHandler : IRequestHandler<RemoveStaffCommand>
{
    private readonly IApplicationDbContext _context;

    public RemoveStaffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RemoveStaffCommand request, CancellationToken cancellationToken)
    {
        var staff = await _context.CourseOfferingStaffs.FindAsync(request.Id, cancellationToken);
        if (staff == null) throw new NotFoundException(nameof(staff), request.Id);

        _context.CourseOfferingStaffs.Remove(staff);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
