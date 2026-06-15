using ErrorOr;
using HRPayroll.Application.Interfaces;
using MediatR;

namespace HRPayroll.Application.Commands.Contracts.AssignContract;

public sealed record AssignContractCommand(
    Guid EmployeeId,
    string ContractType,
    DateOnly SignedDate,
    DateOnly? ExpiryDate,
    decimal BaseSalaryAmount,
    string BaseSalaryCurrency,
    Guid? TaxBracketSetId,
    Guid? SocialInsuranceConfigId) : IRequest<ErrorOr<Guid>>, ISelfManagesTransaction;
