using Application.Common.Interfaces;
using Application.Features.Sessions.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sessions.Queries;

public record ListSessionsQuery(int? ModuleId = null, int? CourseOfferingId = null) : IRequest<List<SessionDto>>;

public class ListSessionsQueryHandler : IRequestHandler<ListSessionsQuery, List<SessionDto>>
{
    private readonly IApplicationDbContext _context;

    public ListSessionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SessionDto>> Handle(ListSessionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Sessions
            .AsNoTracking()
            .Include(s => s.Module)
            .Include(s => s.Section)
            .Include(s => s.OpenedByUser)
            .AsQueryable();

        if (request.ModuleId.HasValue)
            query = query.Where(s => s.ModuleId == request.ModuleId.Value);

        if (request.CourseOfferingId.HasValue)
            query = query.Where(s => s.Module.CourseOfferingId == request.CourseOfferingId.Value);

        var sessions = await query.OrderByDescending(s => s.OpenedAt).ToListAsync(cancellationToken);

        return sessions.Select(s => new SessionDto(
            s.Id,
            s.ModuleId,
            s.Module.Title,
            s.SectionId,
            s.Section?.Name,
            s.Status,
            s.SelectedMethod,
            s.OpenedByUserId,
            s.OpenedByUser.Name,
            s.OpenedAt,
            s.ClosedAt,
            s.CreatedAt)).ToList();
    }
}
