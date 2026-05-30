using Application.Features.Sessions.Commands;
using Application.Features.Sessions.Dtos;
using Application.Features.Sessions.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Authorize(Roles = "Staff,Admin,Student")]
public class SessionsController : BaseController
{
    [HttpPost]
    public async Task<ActionResult<SessionDto>> Open(OpenSessionCommand command, CancellationToken cancellationToken)
    {
        var hasAccess = command.SectionId.HasValue
            ? await HasSectionAccessAsync(command.SectionId.Value, Domain.Enums.CourseOfferingStaffAccessLevel.Assistant, cancellationToken)
                && await DbContext.Modules.AnyAsync(module =>
                    module.Id == command.ModuleId &&
                    DbContext.Sections.Any(section =>
                        section.Id == command.SectionId.Value &&
                        section.CourseOfferingId == module.CourseOfferingId),
                    cancellationToken)
            : await HasModuleAccessAsync(command.ModuleId, Domain.Enums.CourseOfferingStaffAccessLevel.Assistant, cancellationToken);
        if (!hasAccess) return Forbid();

        var cmd = command with { OpenedByUserId = CurrentUserId };
        var dto = await Mediator.Send(cmd, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }

    [HttpPost("{id}/close")]
    public async Task<ActionResult<SessionDto>> Close(int id, CancellationToken cancellationToken)
    {
        if (!await HasSessionAccessAsync(id, Domain.Enums.CourseOfferingStaffAccessLevel.Assistant, cancellationToken)) return Forbid();

        var dto = await Mediator.Send(new CloseSessionCommand(id), cancellationToken);
        return Ok(dto);
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<SessionDto>> Cancel(int id, CancellationToken cancellationToken)
    {
        if (!await HasSessionAccessAsync(id, Domain.Enums.CourseOfferingStaffAccessLevel.Assistant, cancellationToken)) return Forbid();

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
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Student"))
        {
            if (courseOfferingId.HasValue && !await HasOfferingAccessAsync(courseOfferingId.Value, cancellationToken: cancellationToken)) return Forbid();
            if (moduleId.HasValue && !await HasModuleAccessAsync(moduleId.Value, cancellationToken: cancellationToken)) return Forbid();
            if (!courseOfferingId.HasValue && !moduleId.HasValue) return Forbid();
        }

        var sessions = await Mediator.Send(new ListSessionsQuery(moduleId, courseOfferingId), cancellationToken);
        return Ok(sessions);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<SessionDto>> Get(int id, CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Student") && !await HasSessionAccessAsync(id, cancellationToken: cancellationToken)) return Forbid();

        var session = await Mediator.Send(new GetSessionQuery(id), cancellationToken);
        return Ok(session);
    }
}
