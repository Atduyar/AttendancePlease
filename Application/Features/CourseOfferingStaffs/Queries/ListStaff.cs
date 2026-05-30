using Application.Common.Interfaces;
using Application.Features.CourseOfferingStaffs.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CourseOfferingStaffs.Queries;

public record ListStaffQuery(int CourseOfferingId) : IRequest<List<CourseOfferingStaffDto>>;

public class ListStaffQueryValidator : AbstractValidator<ListStaffQuery>
{
    public ListStaffQueryValidator()
    {
        RuleFor(x => x.CourseOfferingId).GreaterThan(0);
    }
}

public class ListStaffQueryHandler : IRequestHandler<ListStaffQuery, List<CourseOfferingStaffDto>>
{
    private readonly IApplicationDbContext _context;

    public ListStaffQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CourseOfferingStaffDto>> Handle(ListStaffQuery request, CancellationToken cancellationToken)
    {
        var staff = await _context.CourseOfferingStaffs
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Section)
            .Where(s => s.CourseOfferingId == request.CourseOfferingId)
            .OrderBy(s => s.Scope)
            .ThenBy(s => s.Section!.Name)
            .ThenBy(s => s.User.Name)
            .ToListAsync(cancellationToken);

        return staff.Select(s => new CourseOfferingStaffDto(
            s.Id,
            s.CourseOfferingId,
            s.SectionId,
            s.Section?.Name,
            s.UserId,
            s.User.Name,
            s.User.Email!,
            s.User.Role.ToString(),
            s.Scope,
            s.AccessLevel,
            s.RoleTitle,
            s.CreatedAt)).ToList();
    }
}
