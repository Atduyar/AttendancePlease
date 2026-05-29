using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected int CurrentUserId
    {
        get
        {
            var userId = User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.Identity?.Name;

            if (!int.TryParse(userId, out var parsedUserId))
                throw new InvalidOperationException("Authenticated user id claim is missing or invalid.");

            return parsedUserId;
        }
    }
}
