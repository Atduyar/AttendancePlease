using Application.Features.Courses.Commands;
using Application.Features.Courses.Dtos;
using Application.Features.Courses.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public class CoursesController : BaseController
{
    [HttpPost]
    public async Task<ActionResult<CourseDto>> Create(CreateCourseCommand command, CancellationToken cancellationToken)
    {
        var dto = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CourseDto>> Update(int id, UpdateCourseCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        var dto = await Mediator.Send(command, cancellationToken);
        return Ok(dto);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteCourseCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<CourseDto>>> List(CancellationToken cancellationToken)
    {
        var courses = await Mediator.Send(new ListCoursesQuery(), cancellationToken);
        return Ok(courses);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<CourseDto>> Get(int id, CancellationToken cancellationToken)
    {
        var course = await Mediator.Send(new GetCourseQuery(id), cancellationToken);
        return Ok(course);
    }
}
