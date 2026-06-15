using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.Employees.TerminateEmployee;

public sealed record TerminateEmployeeCommand(
    Guid EmployeeId,
    DateOnly TerminationDate,
    string Reason) : IRequest<ErrorOr<Success>>;
