using Application.Common.Interfaces;
using Application.Features.AttendanceSessions.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AttendanceSessions.Queries;

/// <summary>
/// Lists all GPS attendance records captured for a specific session.
/// </summary>
public record ListAttendanceSessionRecordsQuery(Guid SessionId) : IRequest<List<AttendanceSessionRecordDto>>;

public class ListAttendanceSessionRecordsQueryHandler
    : IRequestHandler<ListAttendanceSessionRecordsQuery, List<AttendanceSessionRecordDto>>
{
    private readonly IApplicationDbContext _context;

    public ListAttendanceSessionRecordsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AttendanceSessionRecordDto>> Handle(
        ListAttendanceSessionRecordsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.AttendanceRecords
            .AsNoTracking()
            .Where(x => x.SessionId == request.SessionId)
            .OrderBy(x => x.RecordedAt)
            .Select(x => new AttendanceSessionRecordDto(
                x.Id,
                x.SessionId,
                x.UserId,
                x.UserDisplayName,
                x.Latitude,
                x.Longitude,
                x.DistanceMeters,
                x.IsApproved,
                x.RecordedAt))
            .ToListAsync(cancellationToken);
    }
}
