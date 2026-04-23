using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Attendances.Commands;

public record StudentScanAttendanceCommand(int SessionId, int StudentUserId, AttendanceMethod Method) : IRequest<ScanResult>;

public record ScanResult(int AttendanceId, bool SectionSwitched, string Message);

public class StudentScanAttendanceCommandValidator : AbstractValidator<StudentScanAttendanceCommand>
{
    public StudentScanAttendanceCommandValidator()
    {
        RuleFor(x => x.SessionId).GreaterThan(0);
        RuleFor(x => x.StudentUserId).GreaterThan(0);
    }
}

public class StudentScanAttendanceCommandHandler : IRequestHandler<StudentScanAttendanceCommand, ScanResult>
{
    private readonly IApplicationDbContext _context;

    public StudentScanAttendanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ScanResult> Handle(StudentScanAttendanceCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.Sessions
            .AsNoTracking()
            .Include(s => s.Module)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session == null)
            return new ScanResult(0, false, "Session not found");

        if (session.Status != SessionStatus.Open)
            return new ScanResult(0, false, "Session is not open");

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == request.StudentUserId
                && e.CourseOfferingId == session.Module.CourseOfferingId, cancellationToken);

        if (enrollment == null)
            return new ScanResult(0, false, "Not enrolled in this course");

        bool sectionSwitched = false;

        if (session.SectionId.HasValue && enrollment.SectionId != session.SectionId.Value)
        {
            enrollment.SectionId = session.SectionId.Value;
            sectionSwitched = true;
        }

        var attendance = new Attendance
        {
            UserId = request.StudentUserId,
            SessionId = request.SessionId,
            Status = AttendanceStatus.Present,
            Method = request.Method,
            RecordedAt = DateTime.UtcNow
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync(cancellationToken);

        var message = sectionSwitched
            ? "Attendance recorded. Section auto-switched."
            : "Attendance recorded successfully.";

        return new ScanResult(attendance.Id, sectionSwitched, message);
    }
}
