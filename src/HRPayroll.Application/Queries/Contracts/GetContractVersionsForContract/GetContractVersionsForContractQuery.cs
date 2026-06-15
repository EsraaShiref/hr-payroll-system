using ErrorOr;
using HRPayroll.Application.DTOs.Contracts;
using MediatR;

namespace HRPayroll.Application.Queries.Contracts.GetContractVersionsForContract;

public sealed record GetContractVersionsForContractQuery(Guid ContractId)
    : IRequest<ErrorOr<List<ContractVersionDto>>>;
