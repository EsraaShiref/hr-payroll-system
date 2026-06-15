using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.Departments.CreateDepartment;

public sealed record CreateDepartmentCommand(
    string Name,
    string Code,
    string? Description,
    Guid? ParentDepartmentId) : IRequest<ErrorOr<Guid>>;
