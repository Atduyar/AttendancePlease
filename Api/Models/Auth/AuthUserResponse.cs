namespace Api.Models.Auth;

public record AuthUserResponse(
    int Id,
    string Email,
    string Name,
    string Role,
    List<string> Roles);
