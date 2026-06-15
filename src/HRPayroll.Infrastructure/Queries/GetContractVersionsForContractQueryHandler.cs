using ErrorOr;
using HRPayroll.Application.DTOs.Contracts;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Contracts.GetContractVersionsForContract;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries;

public class GetContractVersionsForContractQueryHandler
    : IRequestHandler<GetContractVersionsForContractQuery, ErrorOr<List<ContractVersionDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetContractVersionsForContractQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ErrorOr<List<ContractVersionDto>>> Handle(
        GetContractVersionsForContractQuery query, CancellationToken ct)
    {
        var contractExists = await _db.Contracts
            .AnyAsync(c => c.Id == query.ContractId && !c.IsDeleted, ct);

        if (!contractExists)
            return Error.NotFound("Contract.NotFound",
                $"Contract {query.ContractId} not found.");

        var versions = await _db.ContractVersions
            .AsNoTracking()
            .Where(v => v.ContractId == query.ContractId && !v.IsDeleted)
            .Include(v => v.AllowanceAssignments)
                .ThenInclude(aa => aa.Allowance)
            .OrderByDescending(v => v.VersionNumber)
            .ProjectToType<ContractVersionDto>()
            .ToListAsync(ct);

        return versions;
    }
}
