using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.Contracts.AddContractVersion;

public sealed record AddContractVersionCommand(
    Guid ContractId,
    decimal NewBaseSalaryAmount,
    string NewBaseSalaryCurrency,
    DateOnly EffectiveFrom,
    Guid? TaxBracketSetId,
    Guid? SocialInsuranceConfigId,
    List<AllowanceAssignmentInput>? AllowanceAssignments) : IRequest<ErrorOr<Guid>>;

public sealed record AllowanceAssignmentInput(
    Guid AllowanceId,
    decimal? OverrideAmount,
    decimal? OverridePercentage);
