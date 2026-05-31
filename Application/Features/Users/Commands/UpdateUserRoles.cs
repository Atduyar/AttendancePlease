using Application.Common.Exceptions;
using Application.Features.Users.Dtos;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.Commands;

public record UpdateUserRolesCommand(int Id, List<UserRole> Roles) : IRequest<UserDto>;

public class UpdateUserRolesCommandValidator : AbstractValidator<UpdateUserRolesCommand>
{
    public UpdateUserRolesCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Roles).NotEmpty();
        RuleForEach(x => x.Roles).IsInEnum();
    }
}

public class UpdateUserRolesCommandHandler : IRequestHandler<UpdateUserRolesCommand, UserDto>
{
    private static readonly UserRole[] PrimaryRoleOrder = [UserRole.Admin, UserRole.Staff, UserRole.Student];
    private readonly UserManager<User> _userManager;

    public UpdateUserRolesCommandHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserDto> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null) throw new NotFoundException(nameof(user), request.Id);

        var requestedRoles = request.Roles.Distinct().ToList();
        var requestedRoleNames = requestedRoles.Select(role => role.ToString()).ToArray();
        var appRoleNames = Enum.GetNames<UserRole>();
        var currentRoles = await _userManager.GetRolesAsync(user);
        var currentAppRoles = currentRoles.Where(role => appRoleNames.Contains(role)).ToArray();

        var rolesToRemove = currentAppRoles.Except(requestedRoleNames).ToArray();
        if (rolesToRemove.Length > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded) throw ToValidationException(removeResult, nameof(request.Roles));
        }

        var rolesToAdd = requestedRoleNames.Except(currentAppRoles).ToArray();
        if (rolesToAdd.Length > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded) throw ToValidationException(addResult, nameof(request.Roles));
        }

        user.Role = PrimaryRoleOrder.First(role => requestedRoles.Contains(role));
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded) throw ToValidationException(updateResult, nameof(request.Roles));

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var primaryRole = roles.FirstOrDefault() ?? user.Role.ToString();
        return new UserDto(user.Id, user.Name, user.Email!, user.StudentNumber, primaryRole, roles, user.CreatedAt);
    }

    private static Application.Common.Exceptions.ValidationException ToValidationException(IdentityResult result, string propertyName)
    {
        return new Application.Common.Exceptions.ValidationException(new Dictionary<string, string[]>
        {
            [propertyName] = result.Errors.Select(error => error.Description).ToArray()
        });
    }
}
