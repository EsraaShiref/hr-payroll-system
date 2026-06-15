using ErrorOr;
using HRPayroll.Application.DTOs.Allowances;
using MediatR;

namespace HRPayroll.Application.Queries.Allowances.GetAllowancesList;

public sealed record GetAllowancesListQuery : IRequest<ErrorOr<List<AllowanceDto>>>;
