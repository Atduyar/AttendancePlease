using Application.Common.Interfaces;
using Application.Features.Auth.Dtos;
using FluentValidation;
using MediatR;

namespace Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResult>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.LoginAsync(
            request.Email, request.Password, cancellationToken);
        return new AuthResult(result.Token, result.UserId, result.Email, result.Name, result.Role, result.Errors.Length == 0, result.Errors);
    }
}
