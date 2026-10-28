using IncidentFlow.Application.Features.Auth.Models;

namespace IncidentFlow.Application.Common.Interfaces;

/// <summary>
/// Authentication and token lifecycle only (SRP / ISP).
/// </summary>
public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResultDto> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken cancellationToken = default);
    Task<AuthResultDto> RefreshTokenAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default);
    Task<UserProfileDto?> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default);
}
