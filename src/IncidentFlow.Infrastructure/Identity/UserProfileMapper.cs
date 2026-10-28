using IncidentFlow.Application.Features.Auth.Models;

namespace IncidentFlow.Infrastructure.Identity;

/// <summary>
/// Single responsibility: map Identity users to profile DTOs.
/// </summary>
internal static class UserProfileMapper
{
    public static UserProfileDto ToDto(ApplicationUser user, IList<string> roles) =>
        new(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.FullName,
            roles.ToList(),
            user.IsActive);
}
