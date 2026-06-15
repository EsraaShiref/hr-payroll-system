using ErrorOr;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using HRPayroll.Domain.Enums;
using HRPayroll.Domain.ValueObjects;
using MediatR;

namespace HRPayroll.Application.Commands.Employees.CreateEmployee;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, ErrorOr<Guid>>
{
    private readonly IEmployeeRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IContractRepository _contractRepository;

    public CreateEmployeeCommandHandler(
        IEmployeeRepository repository,
        IUnitOfWork unitOfWork,
        IContractRepository contractRepository)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _contractRepository = contractRepository;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateEmployeeCommand command, CancellationToken ct)
    {
        if (!await _repository.IsEmployeeCodeUniqueAsync(command.EmployeeCode, ct))
            return Error.Conflict("EmployeeCode.Duplicate", $"Employee code '{command.EmployeeCode}' already exists.");

        if (!await _repository.IsNationalIdUniqueAsync(command.NationalId, ct))
            return Error.Conflict("NationalId.Duplicate", $"National ID '{command.NationalId}' already exists.");

        if (!Enum.TryParse<Gender>(command.Gender, true, out var gender))
            return Error.Validation("Gender.Invalid", $"Invalid gender value: {command.Gender}");

        var employee = new Employee(
            EmployeeCode.Create(command.EmployeeCode),
            command.FirstName,
            command.MiddleName,
            command.LastName,
            command.DateOfBirth,
            gender,
            command.NationalId,
            command.DepartmentId,
            command.PositionId,
            command.HireDate);

        _repository.Add(employee);

        var domainEvents = employee.DomainEvents.ToList();
        employee.ClearDomainEvents();
        // Domain events dispatched later via mediator publishing behavior

        await _unitOfWork.SaveChangesAsync(ct);
        return employee.Id;
    }
}
