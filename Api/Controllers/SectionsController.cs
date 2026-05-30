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
    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<ActionResult<SectionDto>> Create(CreateSectionCommand command, CancellationToken cancellationToken)
    {
        if (!await HasOfferingAccessAsync(command.CourseOfferingId, Domain.Enums.CourseOfferingStaffAccessLevel.Instructor, cancellationToken)) return Forbid();

        var dto = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<ActionResult<SectionDto>> Update(int id, UpdateSectionCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        if (!await HasSectionAccessAsync(id, Domain.Enums.CourseOfferingStaffAccessLevel.Instructor, cancellationToken)) return Forbid();

        var dto = await Mediator.Send(command, cancellationToken);
        return Ok(dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!await HasSectionAccessAsync(id, Domain.Enums.CourseOfferingStaffAccessLevel.Instructor, cancellationToken)) return Forbid();

        await Mediator.Send(new DeleteSectionCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<ActionResult<List<SectionDto>>> List([FromQuery] int? courseOfferingId, CancellationToken cancellationToken)
    {
        if (courseOfferingId.HasValue && !await HasOfferingAccessAsync(courseOfferingId.Value, cancellationToken: cancellationToken)) return Forbid();
        if (!courseOfferingId.HasValue && User.IsInRole("Student")) return Forbid();

        var sections = await Mediator.Send(new ListSectionsQuery(courseOfferingId), cancellationToken);
        return Ok(sections);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<ActionResult<SectionDto>> Get(int id, CancellationToken cancellationToken)
    {
        if (!await HasSectionAccessAsync(id, cancellationToken: cancellationToken)) return Forbid();

        var section = await Mediator.Send(new GetSectionQuery(id), cancellationToken);
        return Ok(section);
    }
}
