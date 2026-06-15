using ErrorOr;
using HRPayroll.Application.DTOs.Auth;
using MediatR;

namespace HRPayroll.Application.Commands.Auth.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<ErrorOr<AuthResult>>;
