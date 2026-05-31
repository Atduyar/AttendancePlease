using Application.Features.Users.Commands;
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

    [HttpPut("{id}/roles")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> UpdateRoles(int id, UpdateUserRolesCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();

        var user = await Mediator.Send(command, cancellationToken);
        return Ok(user);
    }

    [HttpPut("{id}/profile")]
    public async Task<ActionResult<UserDto>> UpdateProfile(int id, UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        if (!User.IsInRole("Admin") && id != CurrentUserId) return Forbid();

        var user = await Mediator.Send(command, cancellationToken);
        return Ok(user);
    }

    [HttpPost("{id}/change-password")]
    public async Task<ActionResult> ChangePassword(int id, ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        if (!User.IsInRole("Admin") && id != CurrentUserId) return Forbid();

        await Mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
