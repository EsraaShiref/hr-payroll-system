using ErrorOr;
using HRPayroll.Application.Interfaces;
using MediatR;

namespace HRPayroll.Application.Commands.RequestEmailChange;

public class RequestEmailChangeCommandHandler
    : IRequestHandler<RequestEmailChangeCommand, ErrorOr<Success>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public RequestEmailChangeCommandHandler(
        IEmployeeRepository employeeRepository,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _employeeRepository = employeeRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(RequestEmailChangeCommand command, CancellationToken ct)
    {
        var empId = _currentUser.EmployeeId;
        if (empId is null)
            return Error.Unauthorized("User.NoEmployee", "No linked employee record.");

        var employee = await _employeeRepository.GetByIdAsync(empId.Value, ct);
        if (employee is null)
            return Error.NotFound("Employee.NotFound", "Employee record not found.");

        try
        {
            employee.RequestEmailChange(command.NewEmail);
        }
        catch (Exception ex) when (ex is ArgumentException)
        {
            return Error.Validation("Email.Invalid", ex.Message);
        }

        _employeeRepository.Update(employee);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success;
    }
}
