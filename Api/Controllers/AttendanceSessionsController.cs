using System.Security.Claims;
using Application.Common.Interfaces;
using Application.Features.AttendanceSessions.Commands;
using Application.Features.AttendanceSessions.Dtos;
using Application.Features.AttendanceSessions.Queries;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace Api.Controllers;

/// <summary>
/// Manages GPS-verified attendance sessions and student proximity check-ins.
/// </summary>
[ApiController]
[Route("api/attendance/sessions")]
public class AttendanceSessionsController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IApplicationDbContext _context;
    private readonly IAttendanceQrCodeService _qrCodeService;
    private readonly IAttendanceCheckInRateLimiter _rateLimiter;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AttendanceSessionsController> _logger;

    public AttendanceSessionsController(
        ISender mediator,
        IApplicationDbContext context,
        IAttendanceQrCodeService qrCodeService,
        IAttendanceCheckInRateLimiter rateLimiter,
        IConfiguration configuration,
        ILogger<AttendanceSessionsController> logger)
    {
        _mediator = mediator;
        _context = context;
        _qrCodeService = qrCodeService;
        _rateLimiter = rateLimiter;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new GPS attendance session and returns the generated scan token.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<AttendanceSessionDto>> Create(
        CreateAttendanceSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (!await HasOfferingAccessAsync(request.CourseOfferingId, cancellationToken))
        {
            return Forbid();
        }

        var result = await _mediator.Send(
            new CreateAttendanceSessionCommand(
                request.CourseOfferingId,
                request.Latitude,
                request.Longitude,
                request.RadiusMeters,
                request.DurationMinutes,
                CurrentUserId),
            cancellationToken);

        return CreatedAtAction(nameof(GetByToken), new { sessionToken = result.SessionToken }, result);
    }

    /// <summary>
    /// Returns public session metadata for a scanned QR code without exposing the anchor coordinates.
    /// </summary>
    [HttpGet("{sessionToken}")]
    [AllowAnonymous]
    public async Task<ActionResult<AttendanceSessionMetadataDto>> GetByToken(
        string sessionToken,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAttendanceSessionByTokenQuery(sessionToken), cancellationToken);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Submits a student's GPS coordinates for server-side attendance verification.
    /// </summary>
    [HttpPost("{sessionToken}/checkin")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<AttendanceSessionCheckInResultDto>> CheckIn(
        string sessionToken,
        AttendanceSessionCheckInRequest request,
        CancellationToken cancellationToken)
    {
        if (!_rateLimiter.TryConsume(CurrentUserId, out var retryAfter))
        {
            var minutes = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalMinutes));
            _logger.LogWarning(
                "GPS attendance check-in throttled for user {UserId}. Retry after {RetryAfter}.",
                CurrentUserId,
                retryAfter);

            return StatusCode(
                StatusCodes.Status429TooManyRequests,
                new AttendanceSessionCheckInResultDto(
                    false,
                    null,
                    $"Too many check-in attempts. Please try again in about {minutes} minute(s)."));
        }

        var result = await _mediator.Send(
            new CheckInAttendanceSessionCommand(
                sessionToken,
                CurrentUserId,
                request.Latitude,
                request.Longitude),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Lists all recorded GPS check-in attempts for a specific attendance session.
    /// </summary>
    [HttpGet("{sessionId:guid}/records")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<List<AttendanceSessionRecordDto>>> ListRecords(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var session = await _context.AttendanceSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);

        if (session == null)
        {
            return NotFound();
        }

        if (!await HasOfferingAccessAsync(session.CourseOfferingId, cancellationToken))
        {
            return Forbid();
        }

        var result = await _mediator.Send(new ListAttendanceSessionRecordsQuery(sessionId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Generates a PNG QR code that points students to the frontend attendance check-in page.
    /// </summary>
    [HttpGet("{sessionToken}/qrcode")]
    [Authorize(Roles = "Staff,Admin")]
    [Produces("image/png")]
    public async Task<IActionResult> GetQrCode(string sessionToken, CancellationToken cancellationToken)
    {
        var session = await _context.AttendanceSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SessionToken == sessionToken, cancellationToken);

        if (session == null)
        {
            return NotFound();
        }

        if (!await HasOfferingAccessAsync(session.CourseOfferingId, cancellationToken))
        {
            return Forbid();
        }

        var frontendBaseUrl = ResolveFrontendBaseUrl();
        var attendanceUrl = $"{frontendBaseUrl}/attend/{session.SessionToken}";
        var png = _qrCodeService.GeneratePng(attendanceUrl);

        return File(png, "image/png");
    }

    private int CurrentUserId
    {
        get
        {
            var userId = User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.Identity?.Name;

            if (!int.TryParse(userId, out var parsedUserId))
            {
                throw new InvalidOperationException("Authenticated user id claim is missing or invalid.");
            }

            return parsedUserId;
        }
    }

    private async Task<bool> HasOfferingAccessAsync(int courseOfferingId, CancellationToken cancellationToken)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        if (User.IsInRole("Staff"))
        {
            return true;
        }

        return await _context.CourseOfferingStaffs.AnyAsync(
            x => x.CourseOfferingId == courseOfferingId
                && x.UserId == CurrentUserId
                && x.Scope == CourseOfferingStaffScope.Offering
                && x.AccessLevel <= CourseOfferingStaffAccessLevel.Viewer,
            cancellationToken);
    }

    private string ResolveFrontendBaseUrl()
    {
        if (TryGetHeaderValue(Request.Headers.Origin, out var origin))
        {
            return origin.TrimEnd('/');
        }

        if (TryGetHeaderValue(Request.Headers.Referer, out var referer)
            && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
        {
            return $"{refererUri.Scheme}://{refererUri.Authority}";
        }

        var configured = _configuration["AttendanceGps:FrontendBaseUrl"]
            ?? _configuration["Cors:AllowedOrigins"]
                ?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured.TrimEnd('/');
        }

        return $"{Request.Scheme}://{Request.Host}";
    }

    private static bool TryGetHeaderValue(StringValues values, out string value)
    {
        value = values.FirstOrDefault() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }
}
