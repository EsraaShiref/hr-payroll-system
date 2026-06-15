using ErrorOr;
using HRPayroll.Application.DTOs.Payroll;
using MediatR;

namespace HRPayroll.Application.Queries.Payroll.GetPayrollRunStatus;

public sealed record GetPayrollRunStatusQuery(Guid PayrollRunId)
    : IRequest<ErrorOr<PayrollRunStatusDto>>;
