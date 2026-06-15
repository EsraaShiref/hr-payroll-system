using ErrorOr;
using HRPayroll.Application.DTOs.Auth;
using HRPayroll.Application.Interfaces;
using MediatR;

namespace HRPayroll.Application.Commands.Auth.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ErrorOr<AuthResult>>
{
    private readonly IAuthService _authService;

    public RefreshTokenCommandHandler(IAuthService authService) => _authService = authService;

    public async Task<ErrorOr<AuthResult>> Handle(RefreshTokenCommand command, CancellationToken ct)
        => await _authService.RefreshTokenAsync(command.RefreshToken, ct);
}
