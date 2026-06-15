using ErrorOr;
using HRPayroll.Application.DTOs.Departments;
using HRPayroll.Application.Interfaces;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Application.Queries.Departments.GetDepartmentsList;

public class GetDepartmentsListQueryHandler : IRequestHandler<GetDepartmentsListQuery, ErrorOr<List<DepartmentDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetDepartmentsListQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ErrorOr<List<DepartmentDto>>> Handle(GetDepartmentsListQuery query, CancellationToken ct)
    {
        var departments = await _db.Departments
            .AsNoTracking()
            .Include(d => d.ParentDepartment)
            .Include(d => d.Employees)
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                Description = d.Description,
                ParentDepartmentId = d.ParentDepartmentId,
                ParentDepartmentName = d.ParentDepartment != null ? d.ParentDepartment.Name : null,
                EmployeeCount = d.Employees.Count(e => !e.IsDeleted)
            })
            .OrderBy(d => d.Name)
            .ToListAsync(ct);

        return departments;
    }
}
