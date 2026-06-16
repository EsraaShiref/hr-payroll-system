using ErrorOr;
using HRPayroll.Application.Interfaces;
using MediatR;

namespace HRPayroll.Application.Commands.Attendance.DisputeAttendance;

public class DisputeAttendanceSummaryCommandHandler
    : IRequestHandler<DisputeAttendanceSummaryCommand, ErrorOr<Success>>
{
    private readonly IAttendanceSummaryRepository _summaryRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public DisputeAttendanceSummaryCommandHandler(
        IAttendanceSummaryRepository summaryRepository,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _summaryRepository = summaryRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        DisputeAttendanceSummaryCommand command, CancellationToken ct)
    {
        var empId = _currentUser.EmployeeId;
        if (empId is null)
            return Error.Unauthorized("User.NoEmployee", "No linked employee record.");

        var summary = await _summaryRepository.GetByIdAsync(command.SummaryId, ct);
        if (summary is null)
            return Error.NotFound("Summary.NotFound", "Attendance summary not found.");
        if (summary.EmployeeId != empId.Value)
            return Error.Forbidden("Summary.NotOwned", "You can only dispute your own attendance records.");

        try
        {
            summary.SubmitDispute(command.ClaimedPunchIn, command.ClaimedPunchOut, command.Reason);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Error.Validation("Dispute.Invalid", ex.Message);
        }

        _summaryRepository.Update(summary);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success;
    }
}
