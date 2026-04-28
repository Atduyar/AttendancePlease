using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.Sessions.Commands;

public record OpenSessionCommand(int ModuleId, int? SectionId, AttendanceMethod SelectedMethod, int OpenedByUserId) : IRequest<int>;

public class OpenSessionCommandValidator : AbstractValidator<OpenSessionCommand>
{
    public OpenSessionCommandValidator()
    {
        RuleFor(x => x.ModuleId).GreaterThan(0);
        RuleFor(x => x.OpenedByUserId).GreaterThan(0);
    }
}

public class OpenSessionCommandHandler : IRequestHandler<OpenSessionCommand, int>
{
    private readonly IApplicationDbContext _context;

    public OpenSessionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(OpenSessionCommand request, CancellationToken cancellationToken)
    {
        var session = new Session
        {
            ModuleId = request.ModuleId,
            SectionId = request.SectionId,
            Status = SessionStatus.Open,
            SelectedMethod = request.SelectedMethod,
            OpenedByUserId = request.OpenedByUserId,
            OpenedAt = DateTime.UtcNow
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);
        return session.Id;
    }
}
