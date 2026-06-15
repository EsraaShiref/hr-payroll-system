using ErrorOr;
using HRPayroll.Application.DTOs.Allowances;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Allowances.GetAllowancesList;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries;

public class GetAllowancesListQueryHandler
    : IRequestHandler<GetAllowancesListQuery, ErrorOr<List<AllowanceDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetAllowancesListQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ErrorOr<List<AllowanceDto>>> Handle(
        GetAllowancesListQuery query, CancellationToken ct)
    {
        var allowances = await _db.Allowances
            .AsNoTracking()
            .ProjectToType<AllowanceDto>()
            .ToListAsync(ct);

        return allowances;
    }
}
