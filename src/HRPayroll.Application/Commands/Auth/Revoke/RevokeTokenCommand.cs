using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.Auth.Revoke;

public record RevokeTokenCommand(string RefreshToken) : IRequest<ErrorOr<Success>>;
