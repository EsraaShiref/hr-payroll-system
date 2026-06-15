using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.Attendance.ProcessDailySummaries;

public sealed record ProcessDailySummariesCommand(
    DateOnly FromDate,
    DateOnly ToDate) : IRequest<ErrorOr<int>>; // returns count of summaries created/updated
