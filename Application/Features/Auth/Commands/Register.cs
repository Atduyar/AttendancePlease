using Application.Common.Interfaces;
using Application.Features.Auth.Dtos;
using Domain.Enums;
using FluentValidation;
using MediatR;

namespace Application.Features.Auth.Commands;

public record RegisterCommand(string Name, string Email, string Password, UserRole Role) : IRequest<AuthResult>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.RegisterAsync(
            request.Name, request.Email, request.Password, request.Role, cancellationToken);
        return new AuthResult(result.Token, result.UserId, result.Email, result.Name, result.Role, result.Errors.Length == 0, result.Errors);
    }
}
