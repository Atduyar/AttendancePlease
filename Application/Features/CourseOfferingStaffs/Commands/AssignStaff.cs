using Application.Common.Interfaces;
using Domain.Entities;
using FluentValidation;
using MediatR;

namespace Application.Features.CourseOfferingStaffs.Commands;

public record AssignStaffCommand(int CourseOfferingId, int UserId, string? RoleTitle) : IRequest<int>;

public class AssignStaffCommandValidator : AbstractValidator<AssignStaffCommand>
{
    public AssignStaffCommandValidator()
    {
        RuleFor(x => x.CourseOfferingId).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}

public class AssignStaffCommandHandler : IRequestHandler<AssignStaffCommand, int>
{
    private readonly IApplicationDbContext _context;

    public AssignStaffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(AssignStaffCommand request, CancellationToken cancellationToken)
    {
        var assignment = new CourseOfferingStaff
        {
            CourseOfferingId = request.CourseOfferingId,
            UserId = request.UserId,
            RoleTitle = request.RoleTitle
        };

        _context.CourseOfferingStaffs.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);
        return assignment.Id;
    }
}
