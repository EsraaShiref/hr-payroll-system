using System.Security.Cryptography;
using System.Text;
using ErrorOr;
using HRPayroll.Application.DTOs.Auth;
using HRPayroll.Application.Interfaces;
using HRPayroll.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly Persistence.ApplicationDbContext _db;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        Persistence.ApplicationDbContext db,
        Microsoft.Extensions.Options.IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _db = db;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<ErrorOr<AuthResult>> LoginAsync(string email, string password, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (result.IsLockedOut)
            return Error.Forbidden("Auth.AccountLocked", "Account is locked out. Try again later.");
        if (!result.Succeeded)
            return Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var accessToken = _tokenService.GenerateAccessToken(
            user.Id, user.EmployeeId?.ToString(), user.Email!, roles, claims);

        var (plainToken, tokenHash) = GenerateRefreshTokenValue();

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            FamilyId = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
        };

        _db.Set<RefreshToken>().Add(refreshTokenEntity);
        await _db.SaveChangesAsync(ct);

        return new AuthResult
        {
            AccessToken = accessToken,
            RefreshToken = plainToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
        };
    }

    public async Task<ErrorOr<AuthResult>> RefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        var tokenHash = HashToken(refreshToken);

        var storedToken = await _db.Set<RefreshToken>()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);

        if (storedToken is null)
            return Error.Unauthorized("Auth.InvalidToken", "Refresh token not found.");

        // Breach detection: token was already revoked or replaced
        if (storedToken.IsRevoked || storedToken.ReplacedByTokenHash is not null)
        {
            await RevokeFamilyAsync(storedToken, ct);
            return Error.Unauthorized("Auth.TokenReused",
                "Refresh token was already used. All tokens in this family have been revoked for security.");
        }

        if (storedToken.IsExpired)
            return Error.Unauthorized("Auth.TokenExpired", "Refresh token has expired. Please log in again.");

        // Rotate: revoke old, issue new (same family)
        var user = storedToken.User;
        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var newAccessToken = _tokenService.GenerateAccessToken(
            user.Id, user.EmployeeId?.ToString(), user.Email!, roles, claims);

        var (newPlainToken, newTokenHash) = GenerateRefreshTokenValue();

        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.ReplacedByTokenHash = newTokenHash;

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newTokenHash,
            FamilyId = storedToken.FamilyId,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
        };

        _db.Set<RefreshToken>().Add(newRefreshToken);
        await _db.SaveChangesAsync(ct);

        return new AuthResult
        {
            AccessToken = newAccessToken,
            RefreshToken = newPlainToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
        };
    }

    public async Task<ErrorOr<Success>> RevokeTokenAsync(string refreshToken, CancellationToken ct)
    {
        var tokenHash = HashToken(refreshToken);

        var storedToken = await _db.Set<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);

        if (storedToken is null || storedToken.IsRevoked)
            return Error.Unauthorized("Auth.InvalidToken", "Refresh token not found or already revoked.");

        storedToken.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Result.Success;
    }

    private async Task RevokeFamilyAsync(RefreshToken compromisedToken, CancellationToken ct)
    {
        if (compromisedToken.FamilyId is null) return;

        var familyTokens = await _db.Set<RefreshToken>()
            .Where(rt => rt.FamilyId == compromisedToken.FamilyId && rt.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in familyTokens)
            token.RevokedAt = DateTime.UtcNow;
    }

    private static (string plain, string hash) GenerateRefreshTokenValue()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var plain = Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
        var hash = HashToken(plain);
        return (plain, hash);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
