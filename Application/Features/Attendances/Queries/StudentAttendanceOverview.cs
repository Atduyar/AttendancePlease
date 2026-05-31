using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Attendances.Queries;

public record StudentAttendanceOverviewQuery(int StudentUserId, int CourseOfferingId)
    : IRequest<StudentAttendanceOverview>;

public record StudentAttendanceOverview(
    int TotalModules,
    int PresentCount,
    int LateCount,
    int AbsentCount,
    int ExcusedCount,
    List<ModuleAttendanceSummary> Modules);

public record ModuleAttendanceSummary(
    int ModuleId,
    string ModuleTitle,
    int OrderIndex,
    string? AttendanceStatus,
    string? SectionName,
    int? SessionId,
    DateTime? SessionDate,
    SessionStatus? SessionStatus);

public class StudentAttendanceOverviewQueryHandler
    : IRequestHandler<StudentAttendanceOverviewQuery, StudentAttendanceOverview>
{
    private readonly IApplicationDbContext _context;

    public StudentAttendanceOverviewQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentAttendanceOverview> Handle(
        StudentAttendanceOverviewQuery request, CancellationToken cancellationToken)
    {
        var enrollment = await _context.Enrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == request.StudentUserId
                && e.CourseOfferingId == request.CourseOfferingId, cancellationToken);

        if (enrollment == null) throw new NotFoundException("Enrollment", request.CourseOfferingId);

        var modules = await _context.Modules
            .AsNoTracking()
            .Where(m => m.CourseOfferingId == request.CourseOfferingId)
            .OrderBy(m => m.OrderIndex)
            .ToListAsync(cancellationToken);

        var sessions = await _context.Sessions
            .AsNoTracking()
            .Include(s => s.Module)
            .Include(s => s.Section)
            .Include(s => s.Attendances)
            .Where(s => s.Module.CourseOfferingId == request.CourseOfferingId
                && s.SectionId == enrollment.SectionId
                && s.Status != Domain.Enums.SessionStatus.Canceled)
            .ToListAsync(cancellationToken);

        var attendances = await _context.Attendances
            .AsNoTracking()
            .Where(a => a.UserId == request.StudentUserId
                && sessions.Select(s => s.Id).Contains(a.SessionId))
            .ToListAsync(cancellationToken);

        var moduleSummaries = modules.Select(m =>
        {
            var session = sessions.FirstOrDefault(s => s.ModuleId == m.Id);
            var attendance = session != null
                ? attendances.FirstOrDefault(a => a.SessionId == session.Id)
                : null;

            return new ModuleAttendanceSummary(
                m.Id,
                m.Title,
                m.OrderIndex,
                attendance?.Status.ToString(),
                session?.Section?.Name,
                session?.Id,
                session?.OpenedAt,
                session?.Status);
        }).ToList();

        return new StudentAttendanceOverview(
            modules.Count,
            attendances.Count(a => a.Status == AttendanceStatus.Present),
            attendances.Count(a => a.Status == AttendanceStatus.Late),
            attendances.Count(a => a.Status == AttendanceStatus.Absent),
            attendances.Count(a => a.Status == AttendanceStatus.Excused),
            moduleSummaries);
    }
}
