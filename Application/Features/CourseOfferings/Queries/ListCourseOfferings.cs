using Application.Common.Interfaces;
using Application.Features.CourseOfferings.Dtos;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CourseOfferings.Queries;

public record ListCourseOfferingsQuery : IRequest<List<CourseOfferingDto>>;

public class ListCourseOfferingsQueryHandler : IRequestHandler<ListCourseOfferingsQuery, List<CourseOfferingDto>>
{
    private readonly IApplicationDbContext _context;

    public ListCourseOfferingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CourseOfferingDto>> Handle(ListCourseOfferingsQuery request, CancellationToken cancellationToken)
    {
        var offerings = await _context.CourseOfferings
            .AsNoTracking()
            .Include(o => o.Course)
            .Include(o => o.Term)
            .ToListAsync(cancellationToken);

        return offerings.Select(o => new CourseOfferingDto(
            o.Id,
            o.CourseId,
            o.Course.Code,
            o.Course.Title,
            o.TermId,
            o.Term.Code,
            o.Note,
            o.CreatedAt)).ToList();
    }
}
