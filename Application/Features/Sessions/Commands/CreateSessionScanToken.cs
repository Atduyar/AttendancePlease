using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sessions.Commands;

public record CreateSessionScanTokenCommand(int SessionId) : IRequest<CreateSessionScanTokenResult>;

public record CreateSessionScanTokenResult(string Token, DateTime ExpiresAt);

public class CreateSessionScanTokenCommandValidator : AbstractValidator<CreateSessionScanTokenCommand>
{
    public CreateSessionScanTokenCommandValidator()
    {
        RuleFor(x => x.SessionId).GreaterThan(0);
    }
}

public class CreateSessionScanTokenCommandHandler : IRequestHandler<CreateSessionScanTokenCommand, CreateSessionScanTokenResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IAttendanceScanTokenService _tokens;

    public CreateSessionScanTokenCommandHandler(IApplicationDbContext context, IAttendanceScanTokenService tokens)
    {
        _context = context;
        _tokens = tokens;
    }

    public async Task<CreateSessionScanTokenResult> Handle(CreateSessionScanTokenCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session == null) throw new NotFoundException(nameof(session), request.SessionId);

        if (session.Status != SessionStatus.Open)
            throw new Application.Common.Exceptions.ValidationException(new Dictionary<string, string[]>
            {
                [nameof(request.SessionId)] = ["Only open sessions can create attendance QR tokens."]
            });

        if (session.SelectedMethod is not (AttendanceMethod.Qr or AttendanceMethod.QrWifi or AttendanceMethod.QrPin))
            throw new Application.Common.Exceptions.ValidationException(new Dictionary<string, string[]>
            {
                [nameof(request.SessionId)] = ["This session does not accept QR attendance."]
            });

        var issued = _tokens.Issue(session.Id);
        return new CreateSessionScanTokenResult(issued.Token, issued.ExpiresAt);
    }
}
