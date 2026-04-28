using Application.Common.Interfaces;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.Sessions.Commands;

public record CancelSessionCommand(int Id) : IRequest<bool>;

public class CancelSessionCommandValidator : AbstractValidator<CancelSessionCommand>
{
    public CancelSessionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CancelSessionCommandHandler : IRequestHandler<CancelSessionCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public CancelSessionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CancelSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.Sessions.FindAsync(new object[] { request.Id }, cancellationToken);
        if (session == null) return false;

        session.Status = SessionStatus.Canceled;
        session.ClosedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
