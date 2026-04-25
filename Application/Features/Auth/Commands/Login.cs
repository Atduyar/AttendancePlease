using Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public record LoginResult(string? Token, bool Succeeded, string[] Errors);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (token, errors) = await _identityService.LoginAsync(
            request.Email, request.Password, cancellationToken);
        return new LoginResult(token, errors.Length == 0, errors);
    }
}
