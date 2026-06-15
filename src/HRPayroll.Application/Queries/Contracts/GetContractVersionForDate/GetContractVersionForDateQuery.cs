using ErrorOr;
using HRPayroll.Application.DTOs.Contracts;
using MediatR;

namespace HRPayroll.Application.Queries.Contracts.GetContractVersionForDate;

public sealed record GetContractVersionForDateQuery(
    Guid EmployeeId,
    DateOnly EffectiveDate) : IRequest<ErrorOr<ContractVersionDto>>;
