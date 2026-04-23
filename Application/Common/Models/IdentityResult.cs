namespace Application.Common.Models;

public record IdentityResult(
    string? Token,
    int UserId,
    string Email,
    string Name,
    string Role,
    string[] Errors);
