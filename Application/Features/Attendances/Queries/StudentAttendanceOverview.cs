using Application.Common.Interfaces;
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
    string? SectionName);

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
                attendance?.Status.ToString() ?? "Absent",
                session?.Section?.Name);
        }).ToList();

        return new StudentAttendanceOverview(
            modules.Count,
            attendances.Count(a => a.Status == Domain.Enums.AttendanceStatus.Present),
            attendances.Count(a => a.Status == Domain.Enums.AttendanceStatus.Late),
            modules.Count - attendances.Count,
            attendances.Count(a => a.Status == Domain.Enums.AttendanceStatus.Excused),
            moduleSummaries);
    }
}
