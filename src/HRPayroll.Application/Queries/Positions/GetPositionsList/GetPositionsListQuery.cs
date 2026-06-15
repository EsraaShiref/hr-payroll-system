using ErrorOr;
using HRPayroll.Application.DTOs.Positions;
using MediatR;

namespace HRPayroll.Application.Queries.Positions.GetPositionsList;

public sealed record GetPositionsListQuery : IRequest<ErrorOr<List<PositionDto>>>;
