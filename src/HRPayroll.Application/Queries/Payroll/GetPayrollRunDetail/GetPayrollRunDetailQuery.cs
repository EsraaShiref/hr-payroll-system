using ErrorOr;
using HRPayroll.Application.DTOs.Payroll;
using MediatR;

namespace HRPayroll.Application.Queries.Payroll.GetPayrollRunDetail;

public sealed record GetPayrollRunDetailQuery(Guid PayrollRunId, Guid EmployeeId)
    : IRequest<ErrorOr<PayrollRunDetailDto>>;
