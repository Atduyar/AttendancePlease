using Application.Common.Exceptions;
using Application.Features.Users.Dtos;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.Commands;

public record UpdateProfileCommand(int Id, string Name, string Email) : IRequest<UserDto>;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserDto>
{
    private readonly UserManager<User> _userManager;

    public UpdateProfileCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null) throw new NotFoundException(nameof(User), request.Id);

        user.Name = request.Name;
        user.Email = request.Email;
        user.UserName = request.Email;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new Common.Exceptions.ValidationException(
                result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }));
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var primaryRole = roles.FirstOrDefault() ?? user.Role.ToString();
        return new UserDto(user.Id, user.Name, user.Email!, user.StudentNumber, primaryRole, roles, user.CreatedAt);
    }
}
