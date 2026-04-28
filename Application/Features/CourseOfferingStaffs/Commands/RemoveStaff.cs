using Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Features.CourseOfferingStaffs.Commands;

public record RemoveStaffCommand(int Id) : IRequest<bool>;

public class RemoveStaffCommandValidator : AbstractValidator<RemoveStaffCommand>
{
    public RemoveStaffCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class RemoveStaffCommandHandler : IRequestHandler<RemoveStaffCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RemoveStaffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RemoveStaffCommand request, CancellationToken cancellationToken)
    {
        var staff = await _context.CourseOfferingStaffs.FindAsync(new object[] { request.Id }, cancellationToken);
        if (staff == null) return false;

        _context.CourseOfferingStaffs.Remove(staff);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
