using FluentValidation;
using IncidentFlow.Application.Common.Interfaces;
using IncidentFlow.Application.Common.Models;
using IncidentFlow.Application.Features.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IncidentFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthController(
        IAuthService authService,
        ICurrentUserService currentUser,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _authService = authService;
        _currentUser = currentUser;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthResultDto>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        await _registerValidator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResultDto>.Ok(result, "Registration successful."));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthResultDto>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        await _loginValidator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await _authService.LoginAsync(request, GetIpAddress(), cancellationToken);
        return Ok(ApiResponse<AuthResultDto>.Ok(result, "Login successful."));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthResultDto>>> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(ApiResponse<AuthResultDto>.Fail("Refresh token is required."));
        }

        var result = await _authService.RefreshTokenAsync(request.RefreshToken, GetIpAddress(), cancellationToken);
        return Ok(ApiResponse<AuthResultDto>.Ok(result, "Token refreshed."));
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Logout(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            await _authService.RevokeRefreshTokenAsync(request.RefreshToken, GetIpAddress(), cancellationToken);
        }

        return Ok(ApiResponse<object>.Ok(new { }, "Logged out."));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> Me(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            return Unauthorized(ApiResponse<UserProfileDto>.Fail("Not authenticated."));
        }

        var profile = await _authService.GetUserProfileAsync(_currentUser.UserId, cancellationToken);
        if (profile is null)
        {
            return NotFound(ApiResponse<UserProfileDto>.Fail("User not found."));
        }

        return Ok(ApiResponse<UserProfileDto>.Ok(profile));
    }

    private string? GetIpAddress()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            return forwarded.FirstOrDefault();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}
