using ErrorOr;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using HRPayroll.Domain.ValueObjects;
using MediatR;

namespace HRPayroll.Application.Commands.Contracts.AddContractVersion;

public class AddContractVersionCommandHandler : IRequestHandler<AddContractVersionCommand, ErrorOr<Guid>>
{
    private readonly IContractRepository _contractRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddContractVersionCommandHandler(IContractRepository contractRepository, IUnitOfWork unitOfWork)
    {
        _contractRepository = contractRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Guid>> Handle(AddContractVersionCommand command, CancellationToken ct)
    {
        var contract = await _contractRepository.GetWithVersionsAsync(command.ContractId, ct);
        if (contract is null)
            return Error.NotFound("Contract.NotFound", $"Contract {command.ContractId} not found.");

        var baseSalary = Money.Create(command.NewBaseSalaryAmount, command.NewBaseSalaryCurrency);

        var allowanceAssignments = command.AllowanceAssignments?
            .Select(a => new AllowanceAssignment(a.AllowanceId, a.OverrideAmount, a.OverridePercentage))
            .ToList();

        await using var txn = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var newVersion = contract.AddVersion(
                baseSalary,
                command.EffectiveFrom,
                command.TaxBracketSetId,
                command.SocialInsuranceConfigId,
                allowanceAssignments);

            await _unitOfWork.SaveChangesAsync(ct);
            await txn.CommitAsync(ct);
            return newVersion.Id;
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            await txn.RollbackAsync(ct);
            return Error.Failure("ContractVersion.AddFailed", ex.Message);
        }
    }
}
