using Application.Features.CourseOfferingStaffs.Commands;
using Application.Features.CourseOfferingStaffs.Dtos;
using Application.Features.CourseOfferingStaffs.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize(Roles = "Admin")]
public class CourseOfferingStaffsController : BaseController
{
    [HttpPost]
    public async Task<ActionResult<int>> Assign(AssignStaffCommand command, CancellationToken cancellationToken)
    {
        var id = await Mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Remove(int id, CancellationToken cancellationToken)
    {
        var success = await Mediator.Send(new RemoveStaffCommand(id), cancellationToken);
        return success ? NoContent() : NotFound();
    }

    [HttpGet]
    public async Task<ActionResult<List<CourseOfferingStaffDto>>> List([FromQuery] int courseOfferingId, CancellationToken cancellationToken)
    {
        var staff = await Mediator.Send(new ListStaffQuery(courseOfferingId), cancellationToken);
        return Ok(staff);
    }
}
