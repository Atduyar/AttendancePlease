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
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<AttendanceDto>> Mark(MarkAttendanceCommand command, CancellationToken cancellationToken)
    {
        var dto = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }

    [HttpPost("scan")]
    public async Task<ActionResult<ScanResult>> Scan(StudentScanAttendanceCommand command, CancellationToken cancellationToken)
    {
        var cmd = command with { StudentUserId = CurrentUserId };
        var result = await Mediator.Send(cmd, cancellationToken);
        return Ok(result);
    }

    [HttpGet("session/{sessionId}")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<List<AttendanceDto>>> ListBySession(int sessionId, CancellationToken cancellationToken)
    {
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
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<AttendanceMatrixResult>> Matrix(
        [FromQuery] int courseOfferingId,
        CancellationToken cancellationToken)
    {
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
