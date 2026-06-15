using ErrorOr;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using HRPayroll.Domain.Enums;
using HRPayroll.Domain.ValueObjects;
using MediatR;

namespace HRPayroll.Application.Commands.Contracts.AssignContract;

public class AssignContractCommandHandler : IRequestHandler<AssignContractCommand, ErrorOr<Guid>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignContractCommandHandler(
        IEmployeeRepository employeeRepository,
        IContractRepository contractRepository,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _contractRepository = contractRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Guid>> Handle(AssignContractCommand command, CancellationToken ct)
    {
        var employee = await _employeeRepository.GetWithContractsAsync(command.EmployeeId, ct);
        if (employee is null)
            return Error.NotFound("Employee.NotFound", $"Employee {command.EmployeeId} not found.");

        if (!Enum.TryParse<ContractType>(command.ContractType, true, out var contractType))
            return Error.Validation("ContractType.Invalid", $"Invalid contract type: {command.ContractType}");

        var baseSalary = Money.Create(command.BaseSalaryAmount, command.BaseSalaryCurrency);

        var initialVersion = new ContractVersion(
            1, baseSalary, command.SignedDate, null,
            command.TaxBracketSetId, command.SocialInsuranceConfigId);

        var contract = new Contract(
            command.EmployeeId,
            contractType,
            command.SignedDate,
            command.ExpiryDate,
            initialVersion);

        await using var txn = await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            employee.AssignContract(contract);
            _contractRepository.Add(contract);
            await _unitOfWork.SaveChangesAsync(ct);
            await txn.CommitAsync(ct);
            return contract.Id;
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            await txn.RollbackAsync(ct);
            return Error.Failure("Contract.AssignmentFailed", ex.Message);
        }
    }
}
