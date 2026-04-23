using Application.Features.Auth.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
public class AuthController : BaseController
{
    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return Problem(
                title: "Registration failed",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?> { ["errors"] = result.Errors });
        }

        return Ok(new
        {
            result.Token,
            result.UserId,
            result.Email,
            result.Name,
            result.Role
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        if (!result.Succeeded)
        {
            return Problem(
                title: "Authentication failed",
                statusCode: StatusCodes.Status401Unauthorized,
                extensions: new Dictionary<string, object?> { ["errors"] = result.Errors });
        }

        return Ok(new
        {
            result.Token,
            result.UserId,
            result.Email,
            result.Name,
            result.Role
        });
    }
}
