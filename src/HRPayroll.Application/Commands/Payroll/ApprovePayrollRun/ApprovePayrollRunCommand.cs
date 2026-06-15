using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.Payroll.ApprovePayrollRun;

public sealed record ApprovePayrollRunCommand(Guid PayrollRunId) : IRequest<ErrorOr<Success>>;
