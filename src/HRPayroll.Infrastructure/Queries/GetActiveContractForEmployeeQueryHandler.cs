using ErrorOr;
using HRPayroll.Application.DTOs.Contracts;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Contracts.GetActiveContractForEmployee;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries;

public class GetActiveContractForEmployeeQueryHandler
    : IRequestHandler<GetActiveContractForEmployeeQuery, ErrorOr<ContractDto>>
{
    private readonly IApplicationDbContext _db;

    public GetActiveContractForEmployeeQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ErrorOr<ContractDto>> Handle(GetActiveContractForEmployeeQuery query, CancellationToken ct)
    {
        var contract = await _db.Contracts
            .AsNoTracking()
            .Where(c => c.EmployeeId == query.EmployeeId
                     && c.Status == Domain.Enums.ContractStatus.Active
                     && !c.IsDeleted)
            .Include(c => c.Versions.OrderByDescending(v => v.VersionNumber).Take(1))
                .ThenInclude(v => v.AllowanceAssignments)
                    .ThenInclude(aa => aa.Allowance)
            .FirstOrDefaultAsync(ct);

        if (contract is null)
            return Error.NotFound("Contract.NotFound",
                $"No active contract found for employee {query.EmployeeId}.");

        return contract.Adapt<ContractDto>();
    }
}
