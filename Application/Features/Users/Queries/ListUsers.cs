using Application.Features.Users.Dtos;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Queries;

public record ListUsersQuery : IRequest<List<UserDto>>;

public class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, List<UserDto>>
{
    private readonly UserManager<User> _userManager;

    public ListUsersQueryHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<UserDto>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var result = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            var primaryRole = roles.FirstOrDefault() ?? user.Role.ToString();
            result.Add(new UserDto(user.Id, user.Name, user.Email!, primaryRole, roles, user.CreatedAt));
        }

        return result;
    }
}
