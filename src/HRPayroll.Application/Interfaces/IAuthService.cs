using ErrorOr;
using HRPayroll.Application.DTOs.Auth;

namespace HRPayroll.Application.Interfaces;

public interface IAuthService
{
    Task<ErrorOr<AuthResult>> LoginAsync(string email, string password, CancellationToken ct);
    Task<ErrorOr<AuthResult>> RefreshTokenAsync(string refreshToken, CancellationToken ct);
    Task<ErrorOr<Success>> RevokeTokenAsync(string refreshToken, CancellationToken ct);
}
