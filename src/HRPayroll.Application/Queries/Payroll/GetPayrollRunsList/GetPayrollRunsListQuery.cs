using ErrorOr;
using HRPayroll.Application.DTOs.Payroll;
using MediatR;

namespace HRPayroll.Application.Queries.Payroll.GetPayrollRunsList;

public sealed record GetPayrollRunsListQuery(int Page = 1, int PageSize = 20)
    : IRequest<ErrorOr<PaginatedPayrollRunsDto>>;

public record PaginatedPayrollRunsDto(
    List<PayrollRunListItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
