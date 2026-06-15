using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.Attendance.OverrideAttendanceSummary;

public sealed record OverrideAttendanceSummaryCommand(
    Guid SummaryId,
    TimeOnly? OverridePunchIn,
    TimeOnly? OverridePunchOut,
    string Reason) : IRequest<ErrorOr<Success>>;
