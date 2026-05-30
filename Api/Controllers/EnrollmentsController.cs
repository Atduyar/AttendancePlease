using Application.Features.Enrollments.Commands;
using Application.Features.Enrollments.Dtos;
using Application.Features.Enrollments.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public class EnrollmentsController : BaseController
{
    [HttpPost]
    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<ActionResult<EnrollmentDto>> Enroll(EnrollStudentCommand command, CancellationToken cancellationToken)
    {
        if (!await HasSectionAccessAsync(command.SectionId, Domain.Enums.CourseOfferingStaffAccessLevel.Instructor, cancellationToken)) return Forbid();

        var dto = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}/section")]
    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<ActionResult<EnrollmentDto>> UpdateSection(int id, UpdateEnrollmentSectionCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        if (!await HasEnrollmentAccessAsync(id, Domain.Enums.CourseOfferingStaffAccessLevel.Instructor, cancellationToken)) return Forbid();
        if (!await HasSectionAccessAsync(command.SectionId, Domain.Enums.CourseOfferingStaffAccessLevel.Instructor, cancellationToken)) return Forbid();

        var dto = await Mediator.Send(command, cancellationToken);
        return Ok(dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<ActionResult> Unenroll(int id, CancellationToken cancellationToken)
    {
        if (!await HasEnrollmentAccessAsync(id, Domain.Enums.CourseOfferingStaffAccessLevel.Instructor, cancellationToken)) return Forbid();

        await Mediator.Send(new UnenrollStudentCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<ActionResult<List<EnrollmentDto>>> List(
        [FromQuery] int? courseOfferingId,
        [FromQuery] int? userId,
        CancellationToken cancellationToken)
    {
        if (courseOfferingId.HasValue && !await HasOfferingAccessAsync(courseOfferingId.Value, cancellationToken: cancellationToken)) return Forbid();
        if (!courseOfferingId.HasValue && User.IsInRole("Student")) return Forbid();

        var enrollments = await Mediator.Send(new ListEnrollmentsQuery(courseOfferingId, userId), cancellationToken);
        return Ok(enrollments);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<List<MyEnrollmentDto>>> Mine(CancellationToken cancellationToken)
    {
        var enrollments = await Mediator.Send(new ListMyEnrollmentsQuery(CurrentUserId), cancellationToken);
        return Ok(enrollments);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<ActionResult<EnrollmentDto>> Get(int id, CancellationToken cancellationToken)
    {
        if (!await HasEnrollmentAccessAsync(id, cancellationToken: cancellationToken)) return Forbid();

        var enrollment = await Mediator.Send(new GetEnrollmentQuery(id), cancellationToken);
        return Ok(enrollment);
    }
}
