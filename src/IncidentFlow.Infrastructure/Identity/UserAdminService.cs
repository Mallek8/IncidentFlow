using IncidentFlow.Application.Common.Exceptions;
using IncidentFlow.Application.Common.Interfaces;
using IncidentFlow.Application.Features.Auth.Models;
using IncidentFlow.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IncidentFlow.Infrastructure.Identity;

/// <summary>
/// Admin user administration only (SRP). Authentication lives in <see cref="AuthService"/>.
/// </summary>
public class UserAdminService : IUserAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserAdminService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<UserProfileDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(cancellationToken);

        var result = new List<UserProfileDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(UserProfileMapper.ToDto(user, roles));
        }

        return result;
    }

    public async Task<UserProfileDto> UpdateUserRolesAsync(
        string userId,
        IReadOnlyList<string> roles,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        var distinctRoles = roles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (distinctRoles.Count == 0 || distinctRoles.Any(r => !Roles.All.Contains(r)))
        {
            throw new ValidationException($"Roles must be one or more of: {string.Join(", ", Roles.All)}.");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var toRemove = currentRoles.Except(distinctRoles).ToList();
        var toAdd = distinctRoles.Except(currentRoles).ToList();

        if (toRemove.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, toRemove);
            if (!removeResult.Succeeded)
            {
                throw new ValidationException(removeResult.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
            }
        }

        if (toAdd.Count > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, toAdd);
            if (!addResult.Succeeded)
            {
                throw new ValidationException(addResult.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
            }
        }

        var updatedRoles = await _userManager.GetRolesAsync(user);
        return UserProfileMapper.ToDto(user, updatedRoles);
    }

    public async Task<UserProfileDto> SetUserActiveAsync(
        string userId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        user.IsActive = isActive;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
        }

        var roles = await _userManager.GetRolesAsync(user);
        return UserProfileMapper.ToDto(user, roles);
    }
}
