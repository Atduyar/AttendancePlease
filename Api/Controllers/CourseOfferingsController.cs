using Application.Features.CourseOfferings.Commands;
using Application.Features.CourseOfferings.Dtos;
using Application.Features.CourseOfferings.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize(Roles = "Admin")]
public class CourseOfferingsController : BaseController
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateCourseOfferingCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateCourseOfferingCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        var success = await Mediator.Send(command, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var success = await Mediator.Send(new DeleteCourseOfferingCommand(id), cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpGet]
    public async Task<ActionResult<List<CourseOfferingDto>>> List(CancellationToken cancellationToken)
    {
        var offerings = await Mediator.Send(new ListCourseOfferingsQuery(), cancellationToken);
        return Ok(offerings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CourseOfferingDto?>> Get(int id, CancellationToken cancellationToken)
    {
        var offering = await Mediator.Send(new GetCourseOfferingQuery(id), cancellationToken);
        return Ok(offering);
    }
}
