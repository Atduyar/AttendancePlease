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
    public async Task<ActionResult<int>> Create(CreateTermCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateTermCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        var success = await Mediator.Send(command, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var success = await Mediator.Send(new DeleteTermCommand(id), cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpGet]
    public async Task<ActionResult<List<TermDto>>> List(CancellationToken cancellationToken)
    {
        var terms = await Mediator.Send(new ListTermsQuery(), cancellationToken);
        return Ok(terms);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TermDto?>> Get(int id, CancellationToken cancellationToken)
    {
        var term = await Mediator.Send(new GetTermQuery(id), cancellationToken);
        return Ok(term);
    }
}
