using Application.Common.Interfaces;
using Application.Features.Enrollments.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Enrollments.Queries;

public record ListMyEnrollmentsQuery(int UserId) : IRequest<List<MyEnrollmentDto>>;

public class ListMyEnrollmentsQueryHandler : IRequestHandler<ListMyEnrollmentsQuery, List<MyEnrollmentDto>>
{
    private readonly IApplicationDbContext _context;

    public ListMyEnrollmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MyEnrollmentDto>> Handle(ListMyEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.Section)
            .Include(e => e.CourseOffering)
                .ThenInclude(co => co.Course)
            .Include(e => e.CourseOffering)
                .ThenInclude(co => co.Term)
            .Where(e => e.UserId == request.UserId)
            .OrderBy(e => e.CourseOffering.Course.Code)
            .ThenBy(e => e.Section.Name)
            .ToListAsync(cancellationToken);

        return enrollments.Select(e => new MyEnrollmentDto(
            e.Id,
            e.UserId!.Value,
            e.User!.Name,
            e.CourseOfferingId,
            e.CourseOffering.CourseId,
            e.CourseOffering.Course.Code,
            e.CourseOffering.Course.Title,
            e.CourseOffering.TermId,
            e.CourseOffering.Term.Code,
            e.SectionId,
            e.Section.Name,
            e.CreatedAt)).ToList();
    }
}
