using Application.Features.Terms.Commands;
using Application.Features.Terms.Dtos;
using Application.Features.Terms.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize(Roles = "Admin")]
public class TermsController : BaseController
{
    [HttpPost]
    public async Task<ActionResult<TermDto>> Create(CreateTermCommand command, CancellationToken cancellationToken)
    {
        var dto = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TermDto>> Update(int id, UpdateTermCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        var dto = await Mediator.Send(command, cancellationToken);
        return Ok(dto);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteTermCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<List<TermDto>>> List(CancellationToken cancellationToken)
    {
        var terms = await Mediator.Send(new ListTermsQuery(), cancellationToken);
        return Ok(terms);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TermDto>> Get(int id, CancellationToken cancellationToken)
    {
        var term = await Mediator.Send(new GetTermQuery(id), cancellationToken);
        return Ok(term);
    }
}
