using ErrorOr;
using HRPayroll.Application.DTOs.Auth;
using MediatR;

namespace HRPayroll.Application.Commands.Auth.Login;

public record LoginCommand(string Email, string Password) : IRequest<ErrorOr<AuthResult>>;
