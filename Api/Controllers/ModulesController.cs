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
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ModuleDto>> Create(CreateModuleCommand command, CancellationToken cancellationToken)
    {
        var dto = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ModuleDto>> Update(int id, UpdateModuleCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        var dto = await Mediator.Send(command, cancellationToken);
        return Ok(dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteModuleCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<List<ModuleDto>>> List([FromQuery] int? courseOfferingId, CancellationToken cancellationToken)
    {
        var modules = await Mediator.Send(new ListModulesQuery(courseOfferingId), cancellationToken);
        return Ok(modules);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<ModuleDto>> Get(int id, CancellationToken cancellationToken)
    {
        var module = await Mediator.Send(new GetModuleQuery(id), cancellationToken);
        return Ok(module);
    }
}
