using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(int UserId, string[] Errors)> RegisterAsync(string name, string email, string password, UserRole role, CancellationToken cancellationToken = default);
    Task<(string? Token, string[] Errors)> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
}
