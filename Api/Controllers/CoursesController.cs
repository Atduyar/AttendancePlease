using Application.Features.Courses.Commands;
using Application.Features.Courses.Dtos;
using Application.Features.Courses.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

public class CoursesController : BaseController
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateCourseCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CourseDto?>> Get(int id, CancellationToken cancellationToken)
    {
        var course = await Mediator.Send(new GetCourseQuery(id), cancellationToken);
        return Ok(course);
    }
}
