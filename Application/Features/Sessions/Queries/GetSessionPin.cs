using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Sessions.Queries;

public record GetSessionPinQuery(int SessionId) : IRequest<SessionPinDto>;

public record SessionPinDto(string Pin, int RotationSeconds, int SecondsRemaining, DateTime ExpiresAt);

public class GetSessionPinQueryValidator : AbstractValidator<GetSessionPinQuery>
{
    public GetSessionPinQueryValidator()
    {
        RuleFor(x => x.SessionId).GreaterThan(0);
    }
}

public class GetSessionPinQueryHandler : IRequestHandler<GetSessionPinQuery, SessionPinDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAttendancePinService _pins;

    public GetSessionPinQueryHandler(IApplicationDbContext context, IAttendancePinService pins)
    {
        _context = context;
        _pins = pins;
    }

    public async Task<SessionPinDto> Handle(GetSessionPinQuery request, CancellationToken cancellationToken)
    {
        var session = await _context.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session == null) throw new NotFoundException(nameof(session), request.SessionId);

        if (session.Status != SessionStatus.Open)
            throw new Application.Common.Exceptions.ValidationException(new Dictionary<string, string[]>
            {
                [nameof(request.SessionId)] = ["Only open sessions have an active QR+PIN code."]
            });

        if (session.SelectedMethod != AttendanceMethod.QrPin)
            throw new Application.Common.Exceptions.ValidationException(new Dictionary<string, string[]>
            {
                [nameof(request.SessionId)] = ["This session does not use QR+PIN attendance."]
            });

        var pin = _pins.GetCurrentPin(session);
        return new SessionPinDto(pin.Pin, pin.RotationSeconds, pin.SecondsRemaining, pin.ExpiresAt);
    }
}
