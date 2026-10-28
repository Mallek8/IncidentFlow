using IncidentFlow.Application.Features.Auth.Models;

namespace IncidentFlow.Application.Common.Interfaces;

/// <summary>
/// Admin user management only (SRP / ISP). Authentication lives in <see cref="IAuthService"/>.
/// </summary>
public interface IUserAdminService
{
    Task<IReadOnlyList<UserProfileDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<UserProfileDto> UpdateUserRolesAsync(string userId, IReadOnlyList<string> roles, CancellationToken cancellationToken = default);
    Task<UserProfileDto> SetUserActiveAsync(string userId, bool isActive, CancellationToken cancellationToken = default);
}
