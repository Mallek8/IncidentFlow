namespace IncidentFlow.Application.Features.Auth.Models;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Role = null);

public record LoginRequest(
    string Email,
    string Password);

public record RefreshTokenRequest(
    string RefreshToken);

public record AuthResultDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    UserProfileDto User);

public record UserProfileDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    IReadOnlyList<string> Roles,
    bool IsActive);
