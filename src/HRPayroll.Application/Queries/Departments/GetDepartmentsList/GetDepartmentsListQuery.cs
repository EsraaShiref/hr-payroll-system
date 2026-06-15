using ErrorOr;
using HRPayroll.Application.DTOs.Common;
using HRPayroll.Application.DTOs.Departments;
using MediatR;

namespace HRPayroll.Application.Queries.Departments.GetDepartmentsList;

public sealed record GetDepartmentsListQuery : IRequest<ErrorOr<List<DepartmentDto>>>;
