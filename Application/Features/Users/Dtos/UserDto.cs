namespace Application.Features.Users.Dtos;

public record UserDto(int Id, string Name, string Email, string Role, DateTime CreatedAt);
