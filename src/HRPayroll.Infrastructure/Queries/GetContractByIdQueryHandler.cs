using ErrorOr;
using HRPayroll.Application.DTOs.Contracts;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Contracts.GetContractById;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries;

public class GetContractByIdQueryHandler
    : IRequestHandler<GetContractByIdQuery, ErrorOr<ContractDto>>
{
    private readonly IApplicationDbContext _db;

    public GetContractByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ErrorOr<ContractDto>> Handle(GetContractByIdQuery query, CancellationToken ct)
    {
        var contract = await _db.Contracts
            .AsNoTracking()
            .Include(c => c.Versions.OrderByDescending(v => v.VersionNumber).Take(1))
                .ThenInclude(v => v.AllowanceAssignments)
                    .ThenInclude(aa => aa.Allowance)
            .FirstOrDefaultAsync(c => c.Id == query.Id && !c.IsDeleted, ct);

        if (contract is null)
            return Error.NotFound("Contract.NotFound", $"Contract {query.Id} not found.");

        return contract.Adapt<ContractDto>();
    }
}
