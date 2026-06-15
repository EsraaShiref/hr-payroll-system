using ErrorOr;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Services;
using MediatR;

namespace HRPayroll.Application.Commands.Attendance.ResolveOrphanPunches;

/// <summary>
/// Re-processes a specific employee-date by re-pairing all punches for that day,
/// replacing any existing summary. Used by HR to manually resolve orphan punches.
/// </summary>
public class ResolveOrphanPunchesCommandHandler
    : IRequestHandler<ResolveOrphanPunchesCommand, ErrorOr<Success>>
{
    private readonly IAttendancePunchRepository _punchRepository;
    private readonly IAttendanceSummaryRepository _summaryRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IHolidayRepository _holidayRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PunchPairingService _pairingService;

    public ResolveOrphanPunchesCommandHandler(
        IAttendancePunchRepository punchRepository,
        IAttendanceSummaryRepository summaryRepository,
        IEmployeeRepository employeeRepository,
        ILeaveRequestRepository leaveRequestRepository,
        IHolidayRepository holidayRepository,
        IUnitOfWork unitOfWork,
        PunchPairingService pairingService)
    {
        _punchRepository = punchRepository;
        _summaryRepository = summaryRepository;
        _employeeRepository = employeeRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _holidayRepository = holidayRepository;
        _unitOfWork = unitOfWork;
        _pairingService = pairingService;
    }

    public async Task<ErrorOr<Success>> Handle(ResolveOrphanPunchesCommand command, CancellationToken ct)
    {
        var orphanPunch = await _punchRepository.GetByIdAsync(command.PunchId, ct);
        if (orphanPunch == null)
            return Error.NotFound("Punch.NotFound", "Punch not found.");

        var targetDate = command.TargetDate;
        var employeeId = orphanPunch.EmployeeId;

        // Get all punches for this employee on the target date and re-pair
        var allPunches = await _punchRepository.GetByEmployeeAndDateAsync(employeeId, targetDate, ct);

        // Remove existing summary for this date so ProcessEmployeeDay can recreate it
        var existing = await _summaryRepository.GetByEmployeeAndDateAsync(employeeId, targetDate, ct);
        if (existing != null)
        {
            _summaryRepository.Update(existing);
        }

        // Mark all unprocessed and reprocess
        var employee = await _employeeRepository.GetWithDepartmentAndShiftAsync(employeeId, ct);
        if (employee == null)
            return Error.NotFound("Employee.NotFound", "Employee not found.");

        var shiftId = employee.ShiftId ?? employee.Department?.DefaultShiftId;
        if (shiftId == null)
            return Error.Validation("Shift.NotConfigured", "No shift configured for this employee.");

        var shift = employee.Shift ?? employee.Department?.DefaultShift;
        if (shift == null)
            return Error.Validation("Shift.NotLoaded", "Shift data not available.");

        var pairing = _pairingService.Pair(allPunches);

        foreach (var punch in allPunches)
        {
            if (!pairing.OrphanPunches.Contains(punch))
                punch.MarkProcessed();
        }

        var activeLeaves = await _leaveRequestRepository
            .GetApprovedForEmployeeDateRangeAsync(employeeId, targetDate, targetDate, ct);
        var activeLeave = activeLeaves.FirstOrDefault();
        var holiday = await _holidayRepository.GetByDateAsync(targetDate, ct);

        var summary = new Domain.Entities.AttendanceDailySummary(
            employeeId, targetDate,
            shiftId.Value, shift.Name,
            shift.StartTime, shift.EndTime,
            shift.GracePeriodMinutes, shift.LateThresholdMinutes,
            shift.EarlyDepartureThresholdMinutes, shift.OvertimeThresholdMinutes,
            shift.MinimumWorkMinutesForPresence,
            "System");

        summary.SetPunchData(pairing.FirstPunchIn, pairing.LastPunchOut, pairing.TotalBreakMinutes);
        if (activeLeave != null) summary.MarkAsOnLeave(activeLeave.Id);
        if (holiday != null) summary.MarkAsHoliday(holiday.Id);
        if (pairing.HasOrphans)
            summary.SetNotes($"[Resolved] {pairing.OrphanPunches.Count} orphan(s) remain.");
        summary.Calculate();

        _summaryRepository.Add(summary);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success;
    }
}
