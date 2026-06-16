using ErrorOr;
using HRPayroll.Application.DTOs;
using MediatR;

namespace HRPayroll.Application.Queries.Shifts.GetShiftsList;

public sealed record GetShiftsListQuery : IRequest<ErrorOr<List<ShiftDto>>>;
