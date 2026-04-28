using Application.Features.Enrollments.Commands;
using Application.Features.Enrollments.Dtos;
using Application.Features.Enrollments.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize(Roles = "Admin")]
public class EnrollmentsController : BaseController
{
    [HttpPost]
    public async Task<ActionResult<int>> Enroll(EnrollStudentCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    [HttpPut("{id}/section")]
    public async Task<ActionResult> UpdateSection(int id, UpdateEnrollmentSectionCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        var success = await Mediator.Send(command, cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Unenroll(int id, CancellationToken cancellationToken)
    {
        var success = await Mediator.Send(new UnenrollStudentCommand(id), cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpGet]
    public async Task<ActionResult<List<EnrollmentDto>>> List(
        [FromQuery] int? courseOfferingId,
        [FromQuery] int? userId,
        CancellationToken cancellationToken)
    {
        var enrollments = await Mediator.Send(new ListEnrollmentsQuery(courseOfferingId, userId), cancellationToken);
        return Ok(enrollments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EnrollmentDto?>> Get(int id, CancellationToken cancellationToken)
    {
        var enrollment = await Mediator.Send(new GetEnrollmentQuery(id), cancellationToken);
        return Ok(enrollment);
    }
}
