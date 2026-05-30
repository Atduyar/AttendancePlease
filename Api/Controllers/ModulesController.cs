using Application.Features.Modules.Commands;
using Application.Features.Modules.Dtos;
using Application.Features.Modules.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public class ModulesController : BaseController
{
    [HttpPost]
    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<ActionResult<ModuleDto>> Create(CreateModuleCommand command, CancellationToken cancellationToken)
    {
        if (!await HasOfferingAccessAsync(command.CourseOfferingId, Domain.Enums.CourseOfferingStaffAccessLevel.Instructor, cancellationToken)) return Forbid();

        var dto = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<ActionResult<ModuleDto>> Update(int id, UpdateModuleCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        if (!await HasModuleAccessAsync(id, Domain.Enums.CourseOfferingStaffAccessLevel.Instructor, cancellationToken)) return Forbid();

        var dto = await Mediator.Send(command, cancellationToken);
        return Ok(dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!await HasModuleAccessAsync(id, Domain.Enums.CourseOfferingStaffAccessLevel.Instructor, cancellationToken)) return Forbid();

        await Mediator.Send(new DeleteModuleCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<ActionResult<List<ModuleDto>>> List([FromQuery] int? courseOfferingId, CancellationToken cancellationToken)
    {
        if (courseOfferingId.HasValue && !await HasOfferingAccessAsync(courseOfferingId.Value, cancellationToken: cancellationToken)) return Forbid();
        if (!courseOfferingId.HasValue && User.IsInRole("Student")) return Forbid();

        var modules = await Mediator.Send(new ListModulesQuery(courseOfferingId), cancellationToken);
        return Ok(modules);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<ActionResult<ModuleDto>> Get(int id, CancellationToken cancellationToken)
    {
        if (!await HasModuleAccessAsync(id, cancellationToken: cancellationToken)) return Forbid();

        var module = await Mediator.Send(new GetModuleQuery(id), cancellationToken);
        return Ok(module);
    }
}
