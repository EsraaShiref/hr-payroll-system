using ErrorOr;
using HRPayroll.Application.DTOs.Contracts;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Contracts.GetContractVersionForDate;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries;

public class GetContractVersionForDateQueryHandler
    : IRequestHandler<GetContractVersionForDateQuery, ErrorOr<ContractVersionDto>>
{
    private readonly IApplicationDbContext _db;

    public GetContractVersionForDateQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ErrorOr<ContractVersionDto>> Handle(GetContractVersionForDateQuery query, CancellationToken ct)
    {
        var employeeExists = await _db.Employees.AnyAsync(e => e.Id == query.EmployeeId && !e.IsDeleted, ct);
        if (!employeeExists)
            return Error.NotFound("Employee.NotFound", $"Employee {query.EmployeeId} not found.");

        var version = await _db.ContractVersions
            .AsNoTracking()
            .Where(v => v.Contract.EmployeeId == query.EmployeeId
                     && !v.Contract.IsDeleted
                     && !v.IsDeleted
                     && v.EffectiveFrom <= query.EffectiveDate
                     && (v.EffectiveTo == null || v.EffectiveTo > query.EffectiveDate))
            .Include(v => v.AllowanceAssignments)
                .ThenInclude(aa => aa.Allowance)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(ct);

        if (version is null)
            return Error.NotFound("ContractVersion.NotFound",
                $"No contract version found for employee {query.EmployeeId} on {query.EffectiveDate}.");

        return version.Adapt<ContractVersionDto>();
    }
}
