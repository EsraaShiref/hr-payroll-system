using ErrorOr;
using HRPayroll.Application.Interfaces;
using MediatR;

namespace HRPayroll.Application.Commands.Attendance.OverrideAttendanceSummary;

public class OverrideAttendanceSummaryCommandHandler
    : IRequestHandler<OverrideAttendanceSummaryCommand, ErrorOr<Success>>
{
    private readonly IAttendanceSummaryRepository _summaryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OverrideAttendanceSummaryCommandHandler(
        IAttendanceSummaryRepository summaryRepository,
        IUnitOfWork unitOfWork)
    {
        _summaryRepository = summaryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(OverrideAttendanceSummaryCommand command, CancellationToken ct)
    {
        var summary = await _summaryRepository.GetByIdAsync(command.SummaryId, ct);
        if (summary == null)
            return Error.NotFound("AttendanceSummary.NotFound", "Attendance summary not found.");

        summary.ApplyOverride(command.OverridePunchIn, command.OverridePunchOut, command.Reason);
        _summaryRepository.Update(summary);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success;
    }
}
