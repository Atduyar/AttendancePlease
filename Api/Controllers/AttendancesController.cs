using Application.Features.Attendances.Commands;
using Application.Features.Attendances.Dtos;
using Application.Features.Attendances.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public class AttendancesController : BaseController
{
    [HttpPost("mark")]
    [Authorize(Roles = "Staff,Admin,Student")]
    public async Task<ActionResult<AttendanceDto>> Mark(MarkAttendanceCommand command, CancellationToken cancellationToken)
    {
        if (!await HasSessionAccessAsync(command.SessionId, Domain.Enums.CourseOfferingStaffAccessLevel.Assistant, cancellationToken)) return Forbid();

        var dto = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }

    [HttpGet("scan/preview")]
    [AllowAnonymous]
    public async Task<ActionResult<ScanPreviewDto>> ScanPreview([FromQuery] string token, CancellationToken cancellationToken)
    {
        int? studentUserId = User.Identity?.IsAuthenticated == true && User.IsInRole("Student")
            ? CurrentUserId
            : null;
        var result = await Mediator.Send(new GetScanPreviewQuery(token, studentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("scan/pin")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ScanPinValidationResult>> ValidateScanPin(ValidateScanPinRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new ValidateScanPinCommand(request.Token, request.Pin, CurrentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("scan")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ScanResult>> Scan(StudentScanAttendanceRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new StudentScanAttendanceCommand(request.Token, CurrentUserId, request.Pin), cancellationToken);
        return Ok(result);
    }

    [HttpGet("session/{sessionId}")]
    [Authorize(Roles = "Staff,Admin,Student")]
    public async Task<ActionResult<List<AttendanceDto>>> ListBySession(int sessionId, CancellationToken cancellationToken)
    {
        if (!await HasSessionAccessAsync(sessionId, cancellationToken: cancellationToken)) return Forbid();

        var attendances = await Mediator.Send(new ListAttendancesBySessionQuery(sessionId), cancellationToken);
        return Ok(attendances);
    }

    [HttpGet("overview")]
    public async Task<ActionResult<StudentAttendanceOverview>> MyOverview(
        [FromQuery] int courseOfferingId,
        CancellationToken cancellationToken)
    {
        var overview = await Mediator.Send(new StudentAttendanceOverviewQuery(CurrentUserId, courseOfferingId), cancellationToken);
        return Ok(overview);
    }

    [HttpGet("matrix")]
    [Authorize(Roles = "Staff,Admin,Student")]
    public async Task<ActionResult<AttendanceMatrixResult>> Matrix(
        [FromQuery] int courseOfferingId,
        CancellationToken cancellationToken)
    {
        if (!await HasOfferingAccessAsync(courseOfferingId, cancellationToken: cancellationToken)) return Forbid();

        var result = await Mediator.Send(new AttendanceMatrixQuery(courseOfferingId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AttendanceDto>> Get(int id, CancellationToken cancellationToken)
    {
        var attendance = await Mediator.Send(new GetAttendanceQuery(id), cancellationToken);
        return Ok(attendance);
    }
}
