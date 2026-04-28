using Application.Features.Users.Dtos;
using Domain.Entities;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.Queries;

public record GetUserQuery(int Id) : IRequest<UserDto?>;

public class GetUserQueryValidator : AbstractValidator<GetUserQuery>
{
    public GetUserQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto?>
{
    private readonly UserManager<User> _userManager;

    public GetUserQueryHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserDto?> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        return user?.Adapt<UserDto>();
    }
}
