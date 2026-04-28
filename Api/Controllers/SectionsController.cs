using Application.Features.Sections.Commands;
using Application.Features.Sections.Dtos;
using Application.Features.Sections.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize(Roles = "Admin")]
public class SectionsController : BaseController
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateSectionCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateSectionCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        var success = await Mediator.Send(command, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var success = await Mediator.Send(new DeleteSectionCommand(id), cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpGet]
    public async Task<ActionResult<List<SectionDto>>> List([FromQuery] int? courseOfferingId, CancellationToken cancellationToken)
    {
        var sections = await Mediator.Send(new ListSectionsQuery(courseOfferingId), cancellationToken);
        return Ok(sections);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SectionDto?>> Get(int id, CancellationToken cancellationToken)
    {
        var section = await Mediator.Send(new GetSectionQuery(id), cancellationToken);
        return Ok(section);
    }
}
