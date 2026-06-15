using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.Payroll.PatchRunPayroll;

public sealed record PatchRunPayrollCommand(Guid OriginalRunId, List<Guid> EmployeeIds)
    : IRequest<ErrorOr<Guid>>;
