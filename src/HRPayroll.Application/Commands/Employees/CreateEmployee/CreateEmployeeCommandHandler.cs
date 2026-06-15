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

    public CreateEmployeeCommandHandler(
        IEmployeeRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Guid>> Handle(CreateEmployeeCommand command, CancellationToken ct)
    {
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
