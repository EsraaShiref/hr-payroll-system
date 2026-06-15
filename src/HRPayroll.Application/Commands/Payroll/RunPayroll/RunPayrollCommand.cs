using ErrorOr;
using HRPayroll.Application.Interfaces;
using MediatR;

namespace HRPayroll.Application.Commands.Payroll.RunPayroll;

public sealed record RunPayrollCommand(int Year, int Month, string? Notes)
    : IRequest<ErrorOr<Guid>>;
