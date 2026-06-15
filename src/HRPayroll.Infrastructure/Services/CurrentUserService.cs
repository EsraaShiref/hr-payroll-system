using HRPayroll.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HRPayroll.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId =>
        _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
        ?? _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? "system";

    public string? UserName =>
        _httpContextAccessor.HttpContext?.User.Identity?.Name;

    public string[] Roles =>
        _httpContextAccessor.HttpContext?.User.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToArray() ?? Array.Empty<string>();

    public string[] Claims =>
        _httpContextAccessor.HttpContext?.User.Claims
            .Select(c => $"{c.Type}:{c.Value}")
            .ToArray() ?? Array.Empty<string>();
}
