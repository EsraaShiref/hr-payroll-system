using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.Payroll.RejectPayrollRun;

public sealed record RejectPayrollRunCommand(Guid PayrollRunId, string Reason) : IRequest<ErrorOr<Success>>;
