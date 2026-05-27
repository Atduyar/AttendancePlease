using Application.Features.Sessions.Commands;
using Application.Features.Sessions.Dtos;
using Application.Features.Sessions.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[Authorize]
public class SessionsController : BaseController
{
    [HttpPost]
    public async Task<ActionResult<SessionDto>> Open(OpenSessionCommand command, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var cmd = command with { OpenedByUserId = userId };
        var dto = await Mediator.Send(cmd, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }

    [HttpPost("{id}/close")]
    public async Task<ActionResult<SessionDto>> Close(int id, CancellationToken cancellationToken)
    {
        var dto = await Mediator.Send(new CloseSessionCommand(id), cancellationToken);
        return Ok(dto);
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<SessionDto>> Cancel(int id, CancellationToken cancellationToken)
    {
        var dto = await Mediator.Send(new CancelSessionCommand(id), cancellationToken);
        return Ok(dto);
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
    public async Task<ActionResult<SessionDto>> Get(int id, CancellationToken cancellationToken)
    {
        var session = await Mediator.Send(new GetSessionQuery(id), cancellationToken);
        return Ok(session);
    }
}
