using Application.Features.Users.Dtos;
using Application.Features.Users.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : BaseController
{
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> List(CancellationToken cancellationToken)
    {
        var users = await Mediator.Send(new ListUsersQuery(), cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto?>> Get(int id, CancellationToken cancellationToken)
    {
        var user = await Mediator.Send(new GetUserQuery(id), cancellationToken);
        return Ok(user);
    }
}
