using ErrorOr;
using HRPayroll.Application.DTOs;
using MediatR;

namespace HRPayroll.Application.Queries.Shifts.GetShiftById;

public sealed record GetShiftByIdQuery(Guid Id) : IRequest<ErrorOr<ShiftDto>>;
