using Application.Common;
using Application.Common.Interfaces;
using Application.Features.AttendanceSessions.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.AttendanceSessions.Commands;

/// <summary>
/// Submits a student's GPS position for a QR attendance session.
/// </summary>
public record CheckInAttendanceSessionCommand(
    string SessionToken,
    int UserId,
    double Latitude,
    double Longitude) : IRequest<AttendanceSessionCheckInResultDto>;

public class CheckInAttendanceSessionCommandValidator : AbstractValidator<CheckInAttendanceSessionCommand>
{
    public CheckInAttendanceSessionCommandValidator()
    {
        RuleFor(x => x.SessionToken).NotEmpty();
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Latitude).InclusiveBetween(-90d, 90d);
        RuleFor(x => x.Longitude).InclusiveBetween(-180d, 180d);
    }
}

public class CheckInAttendanceSessionCommandHandler
    : IRequestHandler<CheckInAttendanceSessionCommand, AttendanceSessionCheckInResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CheckInAttendanceSessionCommandHandler> _logger;

    public CheckInAttendanceSessionCommandHandler(
        IApplicationDbContext context,
        ILogger<CheckInAttendanceSessionCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AttendanceSessionCheckInResultDto> Handle(
        CheckInAttendanceSessionCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var session = await _context.AttendanceSessions
            .Include(x => x.CourseOffering)
            .ThenInclude(x => x.Course)
            .FirstOrDefaultAsync(x => x.SessionToken == request.SessionToken, cancellationToken);

        if (session == null)
        {
            _logger.LogWarning(
                "GPS attendance check-in failed because token {Token} was not found for user {UserId}.",
                request.SessionToken,
                request.UserId);
            return new AttendanceSessionCheckInResultDto(false, null, "Attendance session was not found.");
        }

        if (session.ExpiresAt <= now)
        {
            _logger.LogInformation(
                "GPS attendance check-in rejected because session {SessionId} expired for user {UserId}.",
                session.Id,
                request.UserId);
            return new AttendanceSessionCheckInResultDto(false, null, "This attendance session has expired.");
        }

        var existingRecord = await _context.AttendanceRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.SessionId == session.Id && x.UserId == request.UserId,
                cancellationToken);

        if (existingRecord != null)
        {
            _logger.LogInformation(
                "GPS attendance check-in rejected because user {UserId} already checked in for session {SessionId}.",
                request.UserId,
                session.Id);
            return new AttendanceSessionCheckInResultDto(false, existingRecord.DistanceMeters, "Attendance has already been recorded.");
        }

        var student = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

        if (student == null)
        {
            _logger.LogWarning(
                "GPS attendance check-in rejected because user {UserId} no longer exists for session {SessionId}.",
                request.UserId,
                session.Id);
            return new AttendanceSessionCheckInResultDto(false, null, "Authenticated student account could not be resolved.");
        }

        var isEnrolled = await _context.Enrollments.AnyAsync(
            x => x.CourseOfferingId == session.CourseOfferingId && x.UserId == request.UserId,
            cancellationToken);

        if (!isEnrolled)
        {
            _logger.LogInformation(
                "GPS attendance check-in rejected because user {UserId} is not enrolled in offering {CourseOfferingId}.",
                request.UserId,
                session.CourseOfferingId);
            return new AttendanceSessionCheckInResultDto(false, null, "You are not enrolled in this course offering.");
        }

        var distanceMeters = GpsHelper.HaversineDistance(
            session.Latitude,
            session.Longitude,
            request.Latitude,
            request.Longitude);

        var approved = distanceMeters <= session.RadiusMeters;

        var record = new Domain.Entities.AttendanceRecord
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            UserId = request.UserId,
            UserDisplayName = student.Name,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            DistanceMeters = distanceMeters,
            IsApproved = approved,
            RecordedAt = now
        };

        _context.AttendanceRecords.Add(record);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "GPS attendance check-in recorded for user {UserId} in session {SessionId}. Approved: {Approved}. Distance: {DistanceMeters:F2}m.",
            request.UserId,
            session.Id,
            approved,
            distanceMeters);

        return approved
            ? new AttendanceSessionCheckInResultDto(true, distanceMeters, "Attendance recorded.")
            : new AttendanceSessionCheckInResultDto(
                false,
                distanceMeters,
                $"Too far away - you are {Math.Round(distanceMeters, 2)}m from the required location.");
    }
}
