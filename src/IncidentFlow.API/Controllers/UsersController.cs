using IncidentFlow.Application.Common.Interfaces;
using IncidentFlow.Application.Common.Models;
using IncidentFlow.Application.Features.Auth.Models;
using IncidentFlow.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IncidentFlow.API.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public UsersController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserProfileDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserProfileDto>>>> GetUsers(
        CancellationToken cancellationToken)
    {
        var users = await _identityService.GetUsersAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<UserProfileDto>>.Ok(users));
    }

    [HttpPut("{id}/roles")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateRoles(
        string id,
        [FromBody] UpdateUserRolesRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _identityService.UpdateUserRolesAsync(id, request.Roles, cancellationToken);
        return Ok(ApiResponse<UserProfileDto>.Ok(user, "Roles updated."));
    }

    [HttpPut("{id}/active")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> SetActive(
        string id,
        [FromBody] SetUserActiveRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _identityService.SetUserActiveAsync(id, request.IsActive, cancellationToken);
        return Ok(ApiResponse<UserProfileDto>.Ok(user, "User status updated."));
    }
}

public record UpdateUserRolesRequest(IReadOnlyList<string> Roles);
public record SetUserActiveRequest(bool IsActive);
