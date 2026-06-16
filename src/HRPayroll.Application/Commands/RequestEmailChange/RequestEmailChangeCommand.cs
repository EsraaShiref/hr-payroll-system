using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.RequestEmailChange;

public sealed record RequestEmailChangeCommand(string NewEmail) : IRequest<ErrorOr<Success>>;
