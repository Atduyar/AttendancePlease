using Application.Features.Users.Dtos;
using Application.Features.Users.Queries;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Authorize]
public class UsersController : BaseController
{
    [HttpGet]
    [Authorize(Roles = "Admin,Staff,Student")]
    public async Task<ActionResult<List<UserDto>>> List(CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Admin"))
        {
            var hasOwnerAssignment = await DbContext.CourseOfferingStaffs.AnyAsync(
                s => s.UserId == CurrentUserId && s.AccessLevel == CourseOfferingStaffAccessLevel.Owner,
                cancellationToken);
            if (!hasOwnerAssignment) return Forbid();
        }

        var users = await Mediator.Send(new ListUsersQuery(), cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> Get(int id, CancellationToken cancellationToken)
    {
        var user = await Mediator.Send(new GetUserQuery(id), cancellationToken);
        return Ok(user);
    }
}
