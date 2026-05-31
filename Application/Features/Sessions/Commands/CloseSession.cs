using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Sessions.Dtos;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sessions.Commands;

public record CloseSessionCommand(int Id) : IRequest<SessionDto>;

public class CloseSessionCommandValidator : AbstractValidator<CloseSessionCommand>
{
    public CloseSessionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CloseSessionCommandHandler : IRequestHandler<CloseSessionCommand, SessionDto>
{
    private readonly IApplicationDbContext _context;

    public CloseSessionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SessionDto> Handle(CloseSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.Sessions.FindAsync(request.Id, cancellationToken);
        if (session == null) throw new NotFoundException(nameof(session), request.Id);

        session.Status = SessionStatus.Closed;
        session.ClosedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var result = await _context.Sessions
            .AsNoTracking()
            .Include(s => s.Module)
            .Include(s => s.Section)
            .Include(s => s.OpenedByUser)
            .FirstAsync(s => s.Id == session.Id, cancellationToken);

        return SessionDtoMapping.ToDto(result);
    }
}
