using Application.Common.Interfaces;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Attendances.Queries;

public record GetScanPreviewQuery(string Token, int? StudentUserId) : IRequest<ScanPreviewDto>;

public record ScanPreviewDto(
    string CourseCode,
    string CourseTitle,
    string ModuleTitle,
    string? SectionName,
    string OpenedByUserName,
    DateTime OpenedAt,
    SessionStatus Status,
    AttendanceMethod SelectedMethod,
    bool CanSign,
    bool AlreadyRecorded,
    string Message);

public class GetScanPreviewQueryValidator : AbstractValidator<GetScanPreviewQuery>
{
    public GetScanPreviewQueryValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}

public class GetScanPreviewQueryHandler : IRequestHandler<GetScanPreviewQuery, ScanPreviewDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAttendanceScanTokenService _tokens;

    public GetScanPreviewQueryHandler(IApplicationDbContext context, IAttendanceScanTokenService tokens)
    {
        _context = context;
        _tokens = tokens;
    }

    public async Task<ScanPreviewDto> Handle(GetScanPreviewQuery request, CancellationToken cancellationToken)
    {
        var token = _tokens.Validate(request.Token);
        if (!token.IsValid || token.SessionId == null)
        {
            return Invalid(token.Error ?? "Invalid QR code.");
        }

        var session = await _context.Sessions
            .AsNoTracking()
            .Include(s => s.Module)
                .ThenInclude(m => m.CourseOffering)
                    .ThenInclude(o => o.Course)
            .Include(s => s.Section)
            .Include(s => s.OpenedByUser)
            .FirstOrDefaultAsync(s => s.Id == token.SessionId.Value, cancellationToken);

        if (session == null)
            return Invalid("Attendance session was not found.");

        var alreadyRecorded = request.StudentUserId.HasValue && await _context.Attendances
            .AsNoTracking()
            .AnyAsync(a => a.SessionId == session.Id && a.UserId == request.StudentUserId.Value, cancellationToken);

        var sessionCanSign = session.Status == SessionStatus.Open
            && session.SelectedMethod is AttendanceMethod.Qr or AttendanceMethod.QrWifi or AttendanceMethod.QrPin;
        var canSign = sessionCanSign && !alreadyRecorded;
        var message = alreadyRecorded
            ? "Attendance already recorded."
            : sessionCanSign
                ? session.SelectedMethod == AttendanceMethod.QrPin
                    ? "Attendance is open. Confirm your identity, verify the current PIN, then sign below."
                    : "Attendance is open. Confirm your identity, then sign below."
                : session.Status == SessionStatus.Open
                    ? "This session is not accepting QR attendance."
                    : "Attendance has ended. This QR code is no longer accepting check-ins.";

        return new ScanPreviewDto(
            session.Module.CourseOffering.Course.Code,
            session.Module.CourseOffering.Course.Title,
            session.Module.Title,
            session.Section?.Name,
            session.OpenedByUser.Name,
            session.OpenedAt,
            session.Status,
            session.SelectedMethod,
            canSign,
            alreadyRecorded,
            message);
    }

    private static ScanPreviewDto Invalid(string message)
    {
        return new ScanPreviewDto("", "Attendance Check-In", "", null, "", DateTime.UtcNow, SessionStatus.Canceled, AttendanceMethod.Qr, false, false, message);
    }
}
