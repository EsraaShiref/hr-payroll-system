using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.Employees.CreateEmployee;

public sealed record CreateEmployeeCommand(
    string EmployeeCode,
    string FirstName,
    string? MiddleName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    string NationalId,
    Guid DepartmentId,
    Guid PositionId,
    DateOnly HireDate) : IRequest<ErrorOr<Guid>>;
