using Application.Common.Interfaces;
using Application.Features.Sections.Dtos;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sections.Queries;

public record ListSectionsQuery(int? CourseOfferingId = null) : IRequest<List<SectionDto>>;

public class ListSectionsQueryHandler : IRequestHandler<ListSectionsQuery, List<SectionDto>>
{
    private readonly IApplicationDbContext _context;

    public ListSectionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SectionDto>> Handle(ListSectionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Sections.AsNoTracking().AsQueryable();

        if (request.CourseOfferingId.HasValue)
            query = query.Where(s => s.CourseOfferingId == request.CourseOfferingId.Value);

        var sections = await query.ToListAsync(cancellationToken);
        return sections.Adapt<List<SectionDto>>();
    }
}
