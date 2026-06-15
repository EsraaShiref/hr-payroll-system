using ErrorOr;
using HRPayroll.Application.Interfaces;
using MediatR;

namespace HRPayroll.Application.Commands.Auth.Revoke;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, ErrorOr<Success>>
{
    private readonly IAuthService _authService;

    public RevokeTokenCommandHandler(IAuthService authService) => _authService = authService;

    public async Task<ErrorOr<Success>> Handle(RevokeTokenCommand command, CancellationToken ct)
        => await _authService.RevokeTokenAsync(command.RefreshToken, ct);
}
