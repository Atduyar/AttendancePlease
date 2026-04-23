using Application.Common.Interfaces;
using Application.Features.CourseOfferingStaffs.Dtos;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CourseOfferingStaffs.Commands;

public record AssignStaffCommand(int CourseOfferingId, int UserId, string? RoleTitle) : IRequest<CourseOfferingStaffDto>;

public class AssignStaffCommandValidator : AbstractValidator<AssignStaffCommand>
{
    public AssignStaffCommandValidator()
    {
        RuleFor(x => x.CourseOfferingId).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}

public class AssignStaffCommandHandler : IRequestHandler<AssignStaffCommand, CourseOfferingStaffDto>
{
    private readonly IApplicationDbContext _context;

    public AssignStaffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CourseOfferingStaffDto> Handle(AssignStaffCommand request, CancellationToken cancellationToken)
    {
        var assignment = new CourseOfferingStaff
        {
            CourseOfferingId = request.CourseOfferingId,
            UserId = request.UserId,
            RoleTitle = request.RoleTitle
        };

        _context.CourseOfferingStaffs.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        var result = await _context.CourseOfferingStaffs
            .AsNoTracking()
            .Include(s => s.User)
            .FirstAsync(s => s.Id == assignment.Id, cancellationToken);

        return new CourseOfferingStaffDto(
            result.Id,
            result.CourseOfferingId,
            result.UserId,
            result.User.Name,
            result.RoleTitle,
            result.CreatedAt);
    }
}
