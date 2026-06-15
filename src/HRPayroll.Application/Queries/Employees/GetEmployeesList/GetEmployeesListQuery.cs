using ErrorOr;
using HRPayroll.Application.DTOs.Common;
using HRPayroll.Application.DTOs.Employees;
using MediatR;

namespace HRPayroll.Application.Queries.Employees.GetEmployeesList;

public sealed record GetEmployeesListQuery(
    int PageIndex = 0,
    int PageSize = 20,
    string? SortField = null,
    string? SortDirection = "asc",
    string? SearchTerm = null,
    Guid? DepartmentId = null,
    string? EmploymentStatus = null
) : IRequest<ErrorOr<PaginatedList<EmployeeDto>>>;
