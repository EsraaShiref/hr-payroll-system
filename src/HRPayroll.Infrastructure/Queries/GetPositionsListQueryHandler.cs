using ErrorOr;
using HRPayroll.Application.DTOs.Positions;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Queries.Positions.GetPositionsList;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Queries;

public class GetPositionsListQueryHandler
    : IRequestHandler<GetPositionsListQuery, ErrorOr<List<PositionDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetPositionsListQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<ErrorOr<List<PositionDto>>> Handle(
        GetPositionsListQuery query, CancellationToken ct)
    {
        var positions = await _db.Positions
            .AsNoTracking()
            .ProjectToType<PositionDto>()
            .ToListAsync(ct);

        return positions;
    }
}
