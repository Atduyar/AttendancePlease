using Application.Common.Interfaces;
using Application.Features.Enrollments;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Attendances.Commands;

public record StudentScanAttendanceCommand(string Token, int StudentUserId, string? Pin = null) : IRequest<ScanResult>;

public record StudentScanAttendanceRequest(string Token, string? Pin = null);

public record ScanResult(int AttendanceId, bool SectionSwitched, bool AlreadyRecorded, bool Success, string Message);

public class StudentScanAttendanceCommandValidator : AbstractValidator<StudentScanAttendanceCommand>
{
    public StudentScanAttendanceCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.StudentUserId).GreaterThan(0);
    }
}

public class StudentScanAttendanceCommandHandler : IRequestHandler<StudentScanAttendanceCommand, ScanResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IAttendanceScanTokenService _tokens;
    private readonly IAttendancePinService _pins;

    public StudentScanAttendanceCommandHandler(
        IApplicationDbContext context,
        IAttendanceScanTokenService tokens,
        IAttendancePinService pins)
    {
        _context = context;
        _tokens = tokens;
        _pins = pins;
    }

    public async Task<ScanResult> Handle(StudentScanAttendanceCommand request, CancellationToken cancellationToken)
    {
        var token = _tokens.Validate(request.Token);
        if (!token.IsValid || token.SessionId == null)
            return Failed(token.Error ?? "Invalid QR code.");

        var session = await _context.Sessions
            .Include(s => s.Module)
            .FirstOrDefaultAsync(s => s.Id == token.SessionId.Value, cancellationToken);

        if (session == null)
            return Failed("Attendance session was not found.");

        if (session.Status != SessionStatus.Open)
            return Failed("Attendance has ended. This QR code is no longer accepting check-ins.");

        if (session.SelectedMethod is not (AttendanceMethod.Qr or AttendanceMethod.QrWifi or AttendanceMethod.QrPin))
            return Failed("This session is not accepting QR attendance.");

        if (session.SelectedMethod == AttendanceMethod.QrPin && !_pins.ValidatePin(session, request.Pin))
            return Failed("The PIN is missing or no longer valid. Verify the current PIN before signing.");

        var existing = await _context.Attendances
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.SessionId == session.Id && a.UserId == request.StudentUserId, cancellationToken);

        if (existing != null)
            return new ScanResult(existing.Id, false, true, true, "Attendance already recorded.");

        var student = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.StudentUserId, cancellationToken);
        var studentNumber = student?.StudentNumber ?? StudentNumber.FromStudentEmail(student?.Email);
        if (student != null && string.IsNullOrWhiteSpace(student.StudentNumber) && !string.IsNullOrWhiteSpace(studentNumber))
        {
            student.StudentNumber = studentNumber;
        }

        if (!string.IsNullOrWhiteSpace(studentNumber))
        {
            var pendingEnrollments = await _context.Enrollments
                .Where(e => e.UserId == null && e.StudentNumber == studentNumber)
                .ToListAsync(cancellationToken);
            foreach (var pending in pendingEnrollments)
            {
                pending.UserId = request.StudentUserId;
            }
        }

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == request.StudentUserId
                && e.CourseOfferingId == session.Module.CourseOfferingId, cancellationToken);

        if (enrollment == null)
            return Failed("You are not enrolled in this course.");

        bool sectionSwitched = false;

        if (session.SectionId.HasValue && enrollment.SectionId != session.SectionId.Value)
        {
            enrollment.SectionId = session.SectionId.Value;
            sectionSwitched = true;
        }

        var attendance = new Attendance
        {
            UserId = request.StudentUserId,
            SessionId = session.Id,
            Status = AttendanceStatus.Present,
            Method = session.SelectedMethod switch
            {
                AttendanceMethod.QrWifi => AttendanceMethod.QrWifi,
                AttendanceMethod.QrPin => AttendanceMethod.QrPin,
                _ => AttendanceMethod.Qr
            },
            RecordedAt = DateTime.UtcNow
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync(cancellationToken);

        var message = sectionSwitched
            ? "Attendance recorded. Your section was updated for this course."
            : "Attendance recorded successfully.";

        return new ScanResult(attendance.Id, sectionSwitched, false, true, message);
    }

    private static ScanResult Failed(string message) => new(0, false, false, false, message);
}
