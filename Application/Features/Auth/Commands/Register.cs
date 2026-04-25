using Application.Common.Interfaces;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.Auth.Commands;

public record RegisterCommand(string Name, string Email, string Password, UserRole Role) : IRequest<RegisterResult>;

public record RegisterResult(int UserId, bool Succeeded, string[] Errors);

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var (userId, errors) = await _identityService.RegisterAsync(
            request.Name, request.Email, request.Password, request.Role, cancellationToken);
        return new RegisterResult(userId, errors.Length == 0, errors);
    }
}
