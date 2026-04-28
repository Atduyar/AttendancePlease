using Application.Common.Interfaces;
using Application.Features.Enrollments.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Enrollments.Queries;

public record ListEnrollmentsQuery(int? CourseOfferingId = null, int? UserId = null) : IRequest<List<EnrollmentDto>>;

public class ListEnrollmentsQueryHandler : IRequestHandler<ListEnrollmentsQuery, List<EnrollmentDto>>
{
    private readonly IApplicationDbContext _context;

    public ListEnrollmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EnrollmentDto>> Handle(ListEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Enrollments
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.Section)
            .AsQueryable();

        if (request.CourseOfferingId.HasValue)
            query = query.Where(e => e.CourseOfferingId == request.CourseOfferingId.Value);

        if (request.UserId.HasValue)
            query = query.Where(e => e.UserId == request.UserId.Value);

        var enrollments = await query.ToListAsync(cancellationToken);

        return enrollments.Select(e => new EnrollmentDto(
            e.Id,
            e.UserId,
            e.User.Name,
            e.CourseOfferingId,
            e.SectionId,
            e.Section.Name,
            e.CreatedAt)).ToList();
    }
}
