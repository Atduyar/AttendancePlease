using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Attendances.Queries;

public record AttendanceMatrixQuery(int CourseOfferingId)
    : IRequest<AttendanceMatrixResult>;

public record AttendanceMatrixResult(
    List<ModuleHeader> Modules,
    List<StudentRow> Students);

public record ModuleHeader(int ModuleId, string Title, int OrderIndex);

public record StudentRow(
    int StudentId,
    string StudentName,
    int CurrentSectionId,
    string CurrentSectionName,
    List<string?> AttendanceStatuses);

public class AttendanceMatrixQueryHandler
    : IRequestHandler<AttendanceMatrixQuery, AttendanceMatrixResult>
{
    private readonly IApplicationDbContext _context;

    public AttendanceMatrixQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceMatrixResult> Handle(
        AttendanceMatrixQuery request, CancellationToken cancellationToken)
    {
        var modules = await _context.Modules
            .AsNoTracking()
            .Where(m => m.CourseOfferingId == request.CourseOfferingId)
            .OrderBy(m => m.OrderIndex)
            .ToListAsync(cancellationToken);

        var sessions = await _context.Sessions
            .AsNoTracking()
            .Where(s => modules.Select(m => m.Id).Contains(s.ModuleId)
                && s.Status != Domain.Enums.SessionStatus.Canceled)
            .ToListAsync(cancellationToken);

        var students = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.Section)
            .Where(e => e.CourseOfferingId == request.CourseOfferingId)
            .ToListAsync(cancellationToken);

        var sessionIds = sessions.Select(s => s.Id).ToList();
        var attendances = await _context.Attendances
            .AsNoTracking()
            .Where(a => sessionIds.Contains(a.SessionId))
            .ToListAsync(cancellationToken);

        var moduleHeaders = modules
            .Select(m => new ModuleHeader(m.Id, m.Title, m.OrderIndex))
            .ToList();

        var studentRows = students.Select(s =>
        {
            var statuses = modules.Select(m =>
            {
                var session = sessions.FirstOrDefault(sess => sess.ModuleId == m.Id);
                if (session == null) return "N/A";

                var att = attendances.FirstOrDefault(a =>
                    a.SessionId == session.Id && a.UserId == s.UserId);
                return att?.Status.ToString() ?? "Absent";
            }).ToList();

            return new StudentRow(
                s.UserId,
                s.User.Name,
                s.SectionId,
                s.Section.Name,
                statuses.Cast<string?>().ToList());
        }).ToList();

        return new AttendanceMatrixResult(moduleHeaders, studentRows);
    }
}
