using ErrorOr;
using HRPayroll.Application.DTOs.Contracts;
using HRPayroll.Application.DTOs.Employees;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Employees.GetEmployeeById;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries;

public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, ErrorOr<EmployeeDetailDto>>
{
    private readonly IApplicationDbContext _db;

    public GetEmployeeByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ErrorOr<EmployeeDetailDto>> Handle(GetEmployeeByIdQuery query, CancellationToken ct)
    {
        var employee = await _db.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Include(e => e.Contracts.Where(c => c.Status == Domain.Enums.ContractStatus.Active))
                .ThenInclude(c => c.Versions.OrderByDescending(v => v.VersionNumber).Take(1))
                    .ThenInclude(v => v.AllowanceAssignments)
                        .ThenInclude(aa => aa.Allowance)
            .FirstOrDefaultAsync(e => e.Id == query.Id && !e.IsDeleted, ct);

        if (employee is null)
            return Error.NotFound("Employee.NotFound", $"Employee {query.Id} not found.");

        var dto = employee.Adapt<EmployeeDetailDto>();

        var activeContract = employee.ActiveContract;
        if (activeContract != null)
        {
            dto = dto with
            {
                ActiveContract = activeContract.Adapt<ContractDto>()
            };
        }

        return dto;
    }
}
