using Application.Features.Sessions.Commands;
using Application.Features.Sessions.Dtos;
using Application.Features.Sessions.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[Authorize(Roles = "Staff,Admin")]
public class SessionsController : BaseController
{
    [HttpPost]
    public async Task<ActionResult<int>> Open(OpenSessionCommand command, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        var cmd = command with { OpenedByUserId = userId };
        var id = await Mediator.Send(cmd, cancellationToken);
        return Ok(id);
    }

    [HttpPost("{id}/close")]
    public async Task<ActionResult> Close(int id, CancellationToken cancellationToken)
    {
        var success = await Mediator.Send(new CloseSessionCommand(id), cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var success = await Mediator.Send(new CancelSessionCommand(id), cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<SessionDto>>> List(
        [FromQuery] int? moduleId,
        [FromQuery] int? courseOfferingId,
        CancellationToken cancellationToken)
    {
        var sessions = await Mediator.Send(new ListSessionsQuery(moduleId, courseOfferingId), cancellationToken);
        return Ok(sessions);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<SessionDto?>> Get(int id, CancellationToken cancellationToken)
    {
        var session = await Mediator.Send(new GetSessionQuery(id), cancellationToken);
        return Ok(session);
    }
}
