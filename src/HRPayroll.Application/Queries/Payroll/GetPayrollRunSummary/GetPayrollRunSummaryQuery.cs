using ErrorOr;
using HRPayroll.Application.DTOs.Payroll;
using MediatR;

namespace HRPayroll.Application.Queries.Payroll.GetPayrollRunSummary;

public sealed record GetPayrollRunSummaryQuery(Guid PayrollRunId)
    : IRequest<ErrorOr<PayrollRunSummaryDto>>;
