using ErrorOr;
using HRPayroll.Application.DTOs.Auth;
using HRPayroll.Application.Interfaces;
using MediatR;

namespace HRPayroll.Application.Commands.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ErrorOr<AuthResult>>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService) => _authService = authService;

    public async Task<ErrorOr<AuthResult>> Handle(LoginCommand command, CancellationToken ct)
        => await _authService.LoginAsync(command.Email, command.Password, ct);
}
