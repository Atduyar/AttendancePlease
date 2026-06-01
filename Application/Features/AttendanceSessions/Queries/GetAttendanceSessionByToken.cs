using Application.Common.Interfaces;
using Application.Features.AttendanceSessions.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AttendanceSessions.Queries;

/// <summary>
/// Returns public metadata for a GPS attendance session without exposing anchor coordinates.
/// </summary>
public record GetAttendanceSessionByTokenQuery(string SessionToken) : IRequest<AttendanceSessionMetadataDto?>;

public class GetAttendanceSessionByTokenQueryHandler
    : IRequestHandler<GetAttendanceSessionByTokenQuery, AttendanceSessionMetadataDto?>
{
    private readonly IApplicationDbContext _context;

    public GetAttendanceSessionByTokenQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceSessionMetadataDto?> Handle(
        GetAttendanceSessionByTokenQuery request,
        CancellationToken cancellationToken)
    {
        var session = await _context.AttendanceSessions
            .AsNoTracking()
            .Include(x => x.CourseOffering)
            .ThenInclude(x => x.Course)
            .FirstOrDefaultAsync(x => x.SessionToken == request.SessionToken, cancellationToken);

        if (session == null)
        {
            return null;
        }

        return new AttendanceSessionMetadataDto(
            session.Id,
            session.CourseOfferingId,
            $"{session.CourseOffering.Course.Code} - {session.CourseOffering.Course.Title}",
            session.ExpiresAt,
            session.ExpiresAt > DateTime.UtcNow);
    }
}
