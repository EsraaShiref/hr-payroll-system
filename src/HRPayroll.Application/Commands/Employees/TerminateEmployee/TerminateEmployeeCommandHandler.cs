using ErrorOr;
using HRPayroll.Application.Interfaces;
using MediatR;

namespace HRPayroll.Application.Commands.Employees.TerminateEmployee;

public class TerminateEmployeeCommandHandler : IRequestHandler<TerminateEmployeeCommand, ErrorOr<Success>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TerminateEmployeeCommandHandler(IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(TerminateEmployeeCommand command, CancellationToken ct)
    {
        var employee = await _employeeRepository.GetWithContractsAsync(command.EmployeeId, ct);
        if (employee is null)
            return Error.NotFound("Employee.NotFound", $"Employee {command.EmployeeId} not found.");

        await using var txn = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            employee.Terminate(command.TerminationDate, command.Reason);
            await _unitOfWork.SaveChangesAsync(ct);
            await txn.CommitAsync(ct);
            return Result.Success;
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            await txn.RollbackAsync(ct);
            return Error.Failure("Employee.TerminateFailed", ex.Message);
        }
    }
}
