using ErrorOr;
using HRPayroll.Application.Commands.Auth.Login;
using HRPayroll.Application.Commands.Auth.RefreshToken;
using HRPayroll.Application.Commands.Auth.Revoke;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRPayroll.Api.Controllers;

public class AuthController : ApiController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        if (result.IsError)
            return Problem(result.Errors);

        SetRefreshTokenCookie(result.Value.RefreshToken, result.Value.ExpiresAt);

        return Ok(new
        {
            accessToken = result.Value.AccessToken,
            expiresAt = result.Value.ExpiresAt
        });
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized();

        var result = await _mediator.Send(new RefreshTokenCommand(refreshToken), ct);

        if (result.IsError)
            return Problem(result.Errors);

        SetRefreshTokenCookie(result.Value.RefreshToken, result.Value.ExpiresAt);

        return Ok(new
        {
            accessToken = result.Value.AccessToken,
            expiresAt = result.Value.ExpiresAt
        });
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(CancellationToken ct)
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Ok();

        var result = await _mediator.Send(new RevokeTokenCommand(refreshToken), ct);

        Response.Cookies.Delete("refreshToken");

        return result.IsError
            ? Problem(result.Errors)
            : Ok();
    }

    private void SetRefreshTokenCookie(string token, DateTime expiresAt)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt.AddDays(7),
            Path = "/api/auth",
            IsEssential = true,
        };

        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }
}
