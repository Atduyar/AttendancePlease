namespace Application.Features.Auth.Dtos;

public record AuthResult(
    string? Token,
    int UserId,
    string Email,
    string Name,
    string Role,
    bool Succeeded,
    string[] Errors);
