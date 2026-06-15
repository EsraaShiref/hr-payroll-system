namespace HRPayroll.Application.DTOs.Auth;

public record AuthResult
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
