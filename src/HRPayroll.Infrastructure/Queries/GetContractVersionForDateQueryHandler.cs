using ErrorOr;
using HRPayroll.Application.DTOs.Contracts;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Contracts.GetContractVersionForDate;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRPayroll.Infrastructure.Queries;

public class GetContractVersionForDateQueryHandler
    : IRequestHandler<GetContractVersionForDateQuery, ErrorOr<ContractVersionDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<GetContractVersionForDateQueryHandler> _logger;

    public GetContractVersionForDateQueryHandler(IApplicationDbContext db, ILogger<GetContractVersionForDateQueryHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ErrorOr<ContractVersionDto>> Handle(GetContractVersionForDateQuery query, CancellationToken ct)
    {
        var employeeExists = await _db.Employees.AnyAsync(e => e.Id == query.EmployeeId && !e.IsDeleted, ct);
        if (!employeeExists)
            return Error.NotFound("Employee.NotFound", $"Employee {query.EmployeeId} not found.");

        try
        {
            var version = await _db.ContractVersions
                .AsNoTracking()
                .Where(v => v.Contract.EmployeeId == query.EmployeeId
                         && !v.Contract.IsDeleted
                         && !v.IsDeleted
                         && v.EffectiveFrom <= query.EffectiveDate
                         && (v.EffectiveTo == null || v.EffectiveTo > query.EffectiveDate))
                .Include(v => v.AllowanceAssignments)
                    .ThenInclude(aa => aa.Allowance)
                .SingleOrDefaultAsync(ct);

            if (version is null)
                return Error.NotFound("ContractVersion.NotFound",
                    $"No contract version found for employee {query.EmployeeId} on {query.EffectiveDate}.");

            return version.Adapt<ContractVersionDto>();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Sequence contains more than one element"))
        {
            _logger.LogError(ex,
                "Overlapping contract versions detected for employee {EmployeeId} on {Date}. " +
                "Domain invariant violated — multiple versions have overlapping effective ranges.",
                query.EmployeeId, query.EffectiveDate);

            return Error.Unexpected("ContractVersion.Overlapping",
                $"Multiple overlapping contract versions found for employee {query.EmployeeId} on {query.EffectiveDate}. " +
                "This indicates a data integrity issue. Contact system administrator.");
        }
    }
}
