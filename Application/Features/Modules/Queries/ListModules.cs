using Application.Common.Interfaces;
using Application.Features.Modules.Dtos;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Modules.Queries;

public record ListModulesQuery(int? CourseOfferingId = null) : IRequest<List<ModuleDto>>;

public class ListModulesQueryHandler : IRequestHandler<ListModulesQuery, List<ModuleDto>>
{
    private readonly IApplicationDbContext _context;

    public ListModulesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ModuleDto>> Handle(ListModulesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Modules.AsNoTracking().AsQueryable();

        if (request.CourseOfferingId.HasValue)
            query = query.Where(m => m.CourseOfferingId == request.CourseOfferingId.Value);

        var modules = await query.OrderBy(m => m.OrderIndex).ToListAsync(cancellationToken);
        return modules.Adapt<List<ModuleDto>>();
    }
}
