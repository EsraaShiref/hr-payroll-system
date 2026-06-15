using HRPayroll.Application.DTOs.Auth;

namespace HRPayroll.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(string userId, string? employeeId, string email,
        IList<string> roles, IList<System.Security.Claims.Claim> additionalClaims);
}
