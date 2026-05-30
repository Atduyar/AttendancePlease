namespace Application.Features.Users.Dtos;

public record UserDto(int Id, string Name, string Email, string? StudentNumber, string Role, List<string> Roles, DateTime CreatedAt);
