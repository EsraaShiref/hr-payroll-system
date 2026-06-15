using ErrorOr;
using HRPayroll.Application.DTOs.Contracts;
using MediatR;

namespace HRPayroll.Application.Queries.Contracts.GetActiveContractForEmployee;

public sealed record GetActiveContractForEmployeeQuery(Guid EmployeeId) : IRequest<ErrorOr<ContractDto>>;
