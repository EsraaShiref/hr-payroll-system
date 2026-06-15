using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.Payroll.FinalizePayrollRun;

public sealed record FinalizePayrollRunCommand(Guid PayrollRunId) : IRequest<ErrorOr<Success>>;
