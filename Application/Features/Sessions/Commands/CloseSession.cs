using Application.Common.Interfaces;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.Sessions.Commands;

public record CloseSessionCommand(int Id) : IRequest<bool>;

public class CloseSessionCommandValidator : AbstractValidator<CloseSessionCommand>
{
    public CloseSessionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CloseSessionCommandHandler : IRequestHandler<CloseSessionCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public CloseSessionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CloseSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.Sessions.FindAsync(new object[] { request.Id }, cancellationToken);
        if (session == null) return false;

        session.Status = SessionStatus.Closed;
        session.ClosedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
