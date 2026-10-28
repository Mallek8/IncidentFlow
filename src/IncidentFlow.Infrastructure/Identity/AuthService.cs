using IncidentFlow.Application.Common.Exceptions;
using IncidentFlow.Application.Common.Interfaces;
using IncidentFlow.Application.Features.Auth.Models;
using IncidentFlow.Domain.Constants;
using IncidentFlow.Infrastructure.Options;
using IncidentFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IncidentFlow.Infrastructure.Identity;

/// <summary>
/// Authentication and token issuance only (SRP).
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext dbContext,
        ITokenService tokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
        _tokenService = tokenService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            throw new BusinessRuleException("An account with this email already exists.");
        }

        var role = string.IsNullOrWhiteSpace(request.Role) ? Roles.Employee : request.Role!;
        if (!Roles.All.Contains(role))
        {
            throw new ValidationException($"Invalid role '{role}'.");
        }

        if (role is not Roles.Employee)
        {
            throw new ForbiddenAccessException("Self-registration is only allowed for the Employee role.");
        }

        if (!await _roleManager.RoleExistsAsync(role))
        {
            throw new BusinessRuleException($"Role '{role}' is not configured.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            EmailConfirmed = true,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            throw new ValidationException(createResult.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
        }

        await _userManager.AddToRoleAsync(user, role);
        return await IssueTokensAsync(user, ipAddress: null, cancellationToken);
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        user.LastLoginAtUtc = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return await IssueTokensAsync(user, ipAddress, cancellationToken);
    }

    public async Task<AuthResultDto> RefreshTokenAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var stored = await _dbContext.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);

        if (stored is null || !stored.IsActive || stored.User is null || !stored.User.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        stored.RevokedAtUtc = DateTime.UtcNow;
        var newRefresh = _tokenService.CreateRefreshToken();
        stored.ReplacedByToken = newRefresh;

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = stored.UserId,
            Token = newRefresh,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            CreatedByIp = ipAddress
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(stored.User);
        var (accessToken, expires) = _tokenService.CreateAccessToken(stored.User, roles);

        return new AuthResultDto(accessToken, newRefresh, expires, UserProfileMapper.ToDto(stored.User, roles));
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var stored = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);

        if (stored is null || !stored.IsActive)
        {
            return;
        }

        stored.RevokedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        return UserProfileMapper.ToDto(user, roles);
    }

    private async Task<AuthResultDto> IssueTokensAsync(ApplicationUser user, string? ipAddress, CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, expires) = _tokenService.CreateAccessToken(user, roles);
        var refreshTokenValue = _tokenService.CreateRefreshToken();

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            CreatedByIp = ipAddress
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResultDto(accessToken, refreshTokenValue, expires, UserProfileMapper.ToDto(user, roles));
    }
}
