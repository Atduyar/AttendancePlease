using System.Security.Cryptography;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.AttendanceSessions.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.AttendanceSessions.Commands;

/// <summary>
/// Creates a GPS-anchored attendance session for a course offering.
/// </summary>
public record CreateAttendanceSessionCommand(
    int CourseOfferingId,
    double Latitude,
    double Longitude,
    double RadiusMeters,
    int DurationMinutes,
    int CreatedByUserId) : IRequest<AttendanceSessionDto>;

public class CreateAttendanceSessionCommandValidator : AbstractValidator<CreateAttendanceSessionCommand>
{
    public CreateAttendanceSessionCommandValidator()
    {
        RuleFor(x => x.CourseOfferingId).GreaterThan(0);
        RuleFor(x => x.CreatedByUserId).GreaterThan(0);
        RuleFor(x => x.Latitude).InclusiveBetween(-90d, 90d);
        RuleFor(x => x.Longitude).InclusiveBetween(-180d, 180d);
        RuleFor(x => x.RadiusMeters).GreaterThan(0d).LessThanOrEqualTo(500d);
        RuleFor(x => x.DurationMinutes).InclusiveBetween(1, 480);
    }
}

public class CreateAttendanceSessionCommandHandler : IRequestHandler<CreateAttendanceSessionCommand, AttendanceSessionDto>
{
    private readonly IApplicationDbContext _context;

    public CreateAttendanceSessionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceSessionDto> Handle(CreateAttendanceSessionCommand request, CancellationToken cancellationToken)
    {
        var offering = await _context.CourseOfferings
            .Include(x => x.Course)
            .FirstOrDefaultAsync(x => x.Id == request.CourseOfferingId, cancellationToken);

        if (offering == null)
        {
            throw new NotFoundException("CourseOffering", request.CourseOfferingId);
        }

        var token = await GenerateUniqueTokenAsync(cancellationToken);
        var createdAt = DateTime.UtcNow;

        var session = new Domain.Entities.AttendanceSession
        {
            Id = Guid.NewGuid(),
            CourseOfferingId = request.CourseOfferingId,
            CreatedByUserId = request.CreatedByUserId,
            SessionToken = token,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            RadiusMeters = request.RadiusMeters,
            ExpiresAt = createdAt.AddMinutes(request.DurationMinutes),
            CreatedAt = createdAt
        };

        _context.AttendanceSessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        return new AttendanceSessionDto(
            session.Id,
            session.CourseOfferingId,
            $"{offering.Course.Code} - {offering.Course.Title}",
            session.CreatedByUserId,
            session.SessionToken,
            session.Latitude,
            session.Longitude,
            session.RadiusMeters,
            session.ExpiresAt,
            session.CreatedAt,
            session.ExpiresAt > DateTime.UtcNow);
    }

    private async Task<string> GenerateUniqueTokenAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var token = CreateToken();
            var exists = await _context.AttendanceSessions.AnyAsync(x => x.SessionToken == token, cancellationToken);
            if (!exists)
            {
                return token;
            }
        }
    }

    private static string CreateToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
