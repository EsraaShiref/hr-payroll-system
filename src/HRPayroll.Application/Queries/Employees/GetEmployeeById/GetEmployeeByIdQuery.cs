using ErrorOr;
using HRPayroll.Application.DTOs.Employees;
using MediatR;

namespace HRPayroll.Application.Queries.Employees.GetEmployeeById;

public sealed record GetEmployeeByIdQuery(Guid Id) : IRequest<ErrorOr<EmployeeDetailDto>>;
