using ErrorOr;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.ValueObjects;
using MediatR;

namespace HRPayroll.Application.Commands.UpdateMyInfo;

public class UpdateMyInfoCommandHandler
    : IRequestHandler<UpdateMyInfoCommand, ErrorOr<Success>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMyInfoCommandHandler(
        IEmployeeRepository employeeRepository,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(UpdateMyInfoCommand command, CancellationToken ct)
    {
        var empId = _currentUser.EmployeeId;
        if (empId is null)
            return Error.Unauthorized("User.NoEmployee", "No linked employee record.");

        var employee = await _employeeRepository.GetByIdAsync(empId.Value, ct);
        if (employee is null)
            return Error.NotFound("Employee.NotFound", "Employee record not found.");

        Address? address = null;
        if (!string.IsNullOrWhiteSpace(command.Street) || !string.IsNullOrWhiteSpace(command.City))
        {
            address = Address.Create(
                command.Street ?? "",
                command.City ?? "",
                command.State,
                command.PostalCode ?? "",
                command.Country ?? "");
        }

        employee.UpdatePersonalInfo(
            command.PhoneNumber,
            address,
            command.EmergencyContactName,
            command.EmergencyContactPhone);

        _employeeRepository.Update(employee);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success;
    }
}
