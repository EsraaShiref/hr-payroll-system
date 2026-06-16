using ErrorOr;
using HRPayroll.Application.DTOs.Dashboard;
using MediatR;

namespace HRPayroll.Application.Queries.Dashboard.GetMonthlyPayrollBudgetSummary;

public sealed record GetMonthlyPayrollBudgetSummaryQuery(
    int Year,
    int Month) : IRequest<ErrorOr<PayrollBudgetSummaryDto>>;
