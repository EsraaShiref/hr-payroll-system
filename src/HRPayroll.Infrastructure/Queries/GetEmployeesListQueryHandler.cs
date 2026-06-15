using ErrorOr;
using HRPayroll.Application.DTOs.Common;
using HRPayroll.Application.DTOs.Employees;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Employees.GetEmployeesList;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries;

public class GetEmployeesListQueryHandler
    : IRequestHandler<GetEmployeesListQuery, ErrorOr<PaginatedList<EmployeeDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetEmployeesListQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ErrorOr<PaginatedList<EmployeeDto>>> Handle(
        GetEmployeesListQuery query, CancellationToken ct)
    {
        var q = _db.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Where(e => !e.IsDeleted);

        // Search
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm;
            q = q.Where(e => e.FirstName.Contains(term)
                          || e.LastName.Contains(term)
                          || e.EmployeeCode.Value.Contains(term));
        }

        // Filters
        if (query.DepartmentId.HasValue)
            q = q.Where(e => e.DepartmentId == query.DepartmentId.Value);

        if (!string.IsNullOrWhiteSpace(query.EmploymentStatus))
            q = q.Where(e => e.EmploymentStatus.ToString() == query.EmploymentStatus);

        // Count
        var totalCount = await q.CountAsync(ct);

        // Sorting
        q = (query.SortField?.ToLower()) switch
        {
            "employeecode" => query.SortDirection == "desc"
                ? q.OrderByDescending(e => e.EmployeeCode.Value)
                : q.OrderBy(e => e.EmployeeCode.Value),
            "firstname" => query.SortDirection == "desc"
                ? q.OrderByDescending(e => e.FirstName)
                : q.OrderBy(e => e.FirstName),
            "lastname" => query.SortDirection == "desc"
                ? q.OrderByDescending(e => e.LastName)
                : q.OrderBy(e => e.LastName),
            "hiredate" => query.SortDirection == "desc"
                ? q.OrderByDescending(e => e.HireDate)
                : q.OrderBy(e => e.HireDate),
            "status" => query.SortDirection == "desc"
                ? q.OrderByDescending(e => e.EmploymentStatus)
                : q.OrderBy(e => e.EmploymentStatus),
            _ => q.OrderBy(e => e.LastName).ThenBy(e => e.FirstName),
        };

        // Pagination
        var items = await q
            .Skip(query.PageIndex * query.PageSize)
            .Take(query.PageSize)
            .ProjectToType<EmployeeDto>()
            .ToListAsync(ct);

        return new PaginatedList<EmployeeDto>(items, totalCount, query.PageIndex, query.PageSize);
    }
}
