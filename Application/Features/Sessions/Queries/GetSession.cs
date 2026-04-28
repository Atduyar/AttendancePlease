using Application.Common.Interfaces;
using Application.Features.Sessions.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sessions.Queries;

public record GetSessionQuery(int Id) : IRequest<SessionDto?>;

public class GetSessionQueryValidator : AbstractValidator<GetSessionQuery>
{
    public GetSessionQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetSessionQueryHandler : IRequestHandler<GetSessionQuery, SessionDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSessionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SessionDto?> Handle(GetSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await _context.Sessions
            .AsNoTracking()
            .Include(s => s.Module)
            .Include(s => s.Section)
            .Include(s => s.OpenedByUser)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (session == null) return null;

        return new SessionDto(
            session.Id,
            session.ModuleId,
            session.Module.Title,
            session.SectionId,
            session.Section?.Name,
            session.Status,
            session.SelectedMethod,
            session.OpenedByUserId,
            session.OpenedByUser.Name,
            session.OpenedAt,
            session.ClosedAt,
            session.CreatedAt);
    }
}
