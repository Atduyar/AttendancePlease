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
            .Where(s => s.CourseOfferingId == request.CourseOfferingId)
            .ToListAsync(cancellationToken);

        return staff.Select(s => new CourseOfferingStaffDto(
            s.Id,
            s.CourseOfferingId,
            s.UserId,
            s.User.Name,
            s.RoleTitle,
            s.CreatedAt)).ToList();
    }
}
