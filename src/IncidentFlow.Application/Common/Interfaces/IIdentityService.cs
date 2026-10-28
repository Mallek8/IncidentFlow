using IncidentFlow.Application.Common.Models;
using IncidentFlow.Application.Features.Auth.Models;

namespace IncidentFlow.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthResultDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResultDto> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken cancellationToken = default);
    Task<AuthResultDto> RefreshTokenAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default);
    Task<UserProfileDto?> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default);
}
