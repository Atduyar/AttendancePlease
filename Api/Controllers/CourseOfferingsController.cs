using Application.Features.CourseOfferings.Commands;
using Application.Features.CourseOfferings.Dtos;
using Application.Features.CourseOfferings.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public class CourseOfferingsController : BaseController
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CourseOfferingDto>> Create(CreateCourseOfferingCommand command, CancellationToken cancellationToken)
    {
        var dto = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CourseOfferingDto>> Update(int id, UpdateCourseOfferingCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        var dto = await Mediator.Send(command, cancellationToken);
        return Ok(dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteCourseOfferingCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<List<CourseOfferingDto>>> List(
        [FromQuery] int? staffUserId,
        CancellationToken cancellationToken)
    {
        var offerings = await Mediator.Send(new ListCourseOfferingsQuery(staffUserId), cancellationToken);
        return Ok(offerings);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<CourseOfferingDto>> Get(int id, CancellationToken cancellationToken)
    {
        var offering = await Mediator.Send(new GetCourseOfferingQuery(id), cancellationToken);
        return Ok(offering);
    }
}
