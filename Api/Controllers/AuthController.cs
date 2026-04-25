using Application.Features.Auth.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
public class AuthController : BaseController
{
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResult>> Register(RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResult>> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
            return Unauthorized(new { errors = result.Errors });
        return Ok(result);
    }
}
