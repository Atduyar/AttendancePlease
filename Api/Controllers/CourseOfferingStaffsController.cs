using Application.Features.CourseOfferingStaffs.Commands;
using Application.Features.CourseOfferingStaffs.Dtos;
using Application.Features.CourseOfferingStaffs.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public class CourseOfferingStaffsController : BaseController
{
    [HttpPost]
    public async Task<ActionResult<CourseOfferingStaffDto>> Assign(AssignStaffCommand command, CancellationToken cancellationToken)
    {
        var dto = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(null, new { id = dto.Id }, dto);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Remove(int id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new RemoveStaffCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<List<CourseOfferingStaffDto>>> List([FromQuery] int courseOfferingId, CancellationToken cancellationToken)
    {
        var staff = await Mediator.Send(new ListStaffQuery(courseOfferingId), cancellationToken);
        return Ok(staff);
    }
}
