using Application.Features.CourseOfferingStaffs.Commands;
using Application.Features.CourseOfferingStaffs.Dtos;
using Application.Features.CourseOfferingStaffs.Queries;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Authorize]
public class CourseOfferingStaffsController : BaseController
{
    [HttpPost]
    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<ActionResult<CourseOfferingStaffDto>> Assign(AssignStaffCommand command, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Admin"))
        {
            if (command.AccessLevel == CourseOfferingStaffAccessLevel.Owner) return Forbid();

            var canManage = command.Scope == CourseOfferingStaffScope.Section && command.SectionId.HasValue
                ? await HasSectionAccessAsync(command.SectionId.Value, CourseOfferingStaffAccessLevel.Owner, cancellationToken, allowGlobalStaffRole: false)
                : await HasOfferingAccessAsync(command.CourseOfferingId, CourseOfferingStaffAccessLevel.Owner, cancellationToken, allowGlobalStaffRole: false);
            if (!canManage) return Forbid();
        }

        var dto = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(null, new { id = dto.Id }, dto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<ActionResult> Remove(int id, CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Admin"))
        {
            var assignment = await DbContext.CourseOfferingStaffs.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            if (assignment == null) return NotFound();
            if (assignment.AccessLevel == CourseOfferingStaffAccessLevel.Owner) return Forbid();

            var canManage = assignment.Scope == CourseOfferingStaffScope.Section && assignment.SectionId.HasValue
                ? await HasSectionAccessAsync(assignment.SectionId.Value, CourseOfferingStaffAccessLevel.Owner, cancellationToken, allowGlobalStaffRole: false)
                : await HasOfferingAccessAsync(assignment.CourseOfferingId, CourseOfferingStaffAccessLevel.Owner, cancellationToken, allowGlobalStaffRole: false);
            if (!canManage) return Forbid();
        }

        await Mediator.Send(new RemoveStaffCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<ActionResult<List<CourseOfferingStaffDto>>> List([FromQuery] int courseOfferingId, CancellationToken cancellationToken)
    {
        if (!await HasOfferingAccessAsync(courseOfferingId, cancellationToken: cancellationToken)) return Forbid();

        var staff = await Mediator.Send(new ListStaffQuery(courseOfferingId), cancellationToken);
        return Ok(staff);
    }
}
