using ErrorOr;
using HRPayroll.Application.DTOs.Contracts;
using MediatR;

namespace HRPayroll.Application.Queries.Contracts.GetContractById;

public sealed record GetContractByIdQuery(Guid Id) : IRequest<ErrorOr<ContractDto>>;
