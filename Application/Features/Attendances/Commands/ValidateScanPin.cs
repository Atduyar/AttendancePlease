using Application.Common.Interfaces;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Attendances.Commands;

public record ValidateScanPinCommand(string Token, string Pin, int StudentUserId) : IRequest<ScanPinValidationResult>;

public record ValidateScanPinRequest(string Token, string Pin);

public record ScanPinValidationResult(bool Success, string Message);

public class ValidateScanPinCommandValidator : AbstractValidator<ValidateScanPinCommand>
{
    public ValidateScanPinCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.Pin).NotEmpty().Matches("^\\d{6}$").WithMessage("PIN must be 6 digits.");
        RuleFor(x => x.StudentUserId).GreaterThan(0);
    }
}

public class ValidateScanPinCommandHandler : IRequestHandler<ValidateScanPinCommand, ScanPinValidationResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IAttendanceScanTokenService _tokens;
    private readonly IAttendancePinService _pins;

    public ValidateScanPinCommandHandler(
        IApplicationDbContext context,
        IAttendanceScanTokenService tokens,
        IAttendancePinService pins)
    {
        _context = context;
        _tokens = tokens;
        _pins = pins;
    }

    public async Task<ScanPinValidationResult> Handle(ValidateScanPinCommand request, CancellationToken cancellationToken)
    {
        var token = _tokens.Validate(request.Token);
        if (!token.IsValid || token.SessionId == null)
            return Failed(token.Error ?? "Invalid QR code.");

        var session = await _context.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == token.SessionId.Value, cancellationToken);

        if (session == null)
            return Failed("Attendance session was not found.");

        if (session.Status != SessionStatus.Open)
            return Failed("Attendance has ended. This QR code is no longer accepting check-ins.");

        if (session.SelectedMethod != AttendanceMethod.QrPin)
            return Failed("This session does not require a PIN.");

        var alreadyRecorded = await _context.Attendances
            .AsNoTracking()
            .AnyAsync(a => a.SessionId == session.Id && a.UserId == request.StudentUserId, cancellationToken);

        if (alreadyRecorded)
            return new ScanPinValidationResult(true, "Attendance already recorded.");

        return _pins.ValidatePin(session, request.Pin)
            ? new ScanPinValidationResult(true, "PIN verified. You can sign attendance now.")
            : Failed("Incorrect PIN. Ask your instructor for the current number and try again.");
    }

    private static ScanPinValidationResult Failed(string message) => new(false, message);
}
