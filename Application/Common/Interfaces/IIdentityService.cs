using Application.Common.Models;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IIdentityService
{
    Task<IdentityResult> RegisterAsync(string name, string email, string password, UserRole role, CancellationToken cancellationToken = default);
    Task<IdentityResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<string[]> AddRoleAsync(string email, UserRole role, CancellationToken cancellationToken = default);
}
