using Application.Features.Modules.Commands;
using Application.Features.Modules.Dtos;
using Application.Features.Modules.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize(Roles = "Admin")]
public class ModulesController : BaseController
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateModuleCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateModuleCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        var success = await Mediator.Send(command, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var success = await Mediator.Send(new DeleteModuleCommand(id), cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpGet]
    public async Task<ActionResult<List<ModuleDto>>> List([FromQuery] int? courseOfferingId, CancellationToken cancellationToken)
    {
        var modules = await Mediator.Send(new ListModulesQuery(courseOfferingId), cancellationToken);
        return Ok(modules);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ModuleDto?>> Get(int id, CancellationToken cancellationToken)
    {
        var module = await Mediator.Send(new GetModuleQuery(id), cancellationToken);
        return Ok(module);
    }
}
