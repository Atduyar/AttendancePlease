using Application.Features.Sections.Commands;
using Application.Features.Sections.Dtos;
using Application.Features.Sections.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public class SectionsController : BaseController
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SectionDto>> Create(CreateSectionCommand command, CancellationToken cancellationToken)
    {
        var dto = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SectionDto>> Update(int id, UpdateSectionCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        var dto = await Mediator.Send(command, cancellationToken);
        return Ok(dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteSectionCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<List<SectionDto>>> List([FromQuery] int? courseOfferingId, CancellationToken cancellationToken)
    {
        var sections = await Mediator.Send(new ListSectionsQuery(courseOfferingId), cancellationToken);
        return Ok(sections);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<SectionDto>> Get(int id, CancellationToken cancellationToken)
    {
        var section = await Mediator.Send(new GetSectionQuery(id), cancellationToken);
        return Ok(section);
    }
}
