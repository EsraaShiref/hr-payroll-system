using System.Collections.Concurrent;
using ErrorOr;
using HRPayroll.Application.Interfaces;
using HRPayroll.Application.Services;
using HRPayroll.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRPayroll.Application.Commands.Attendance.ProcessDailySummaries;

public class ProcessDailySummariesCommandHandler
    : IRequestHandler<ProcessDailySummariesCommand, ErrorOr<int>>
{
    private readonly IAttendancePunchRepository _punchRepository;
    private readonly IAttendanceSummaryRepository _summaryRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IHolidayRepository _holidayRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PunchPairingService _pairingService;
    private readonly ILogger<ProcessDailySummariesCommandHandler> _logger;

    public ProcessDailySummariesCommandHandler(
        IAttendancePunchRepository punchRepository,
        IAttendanceSummaryRepository summaryRepository,
        IEmployeeRepository employeeRepository,
        ILeaveRequestRepository leaveRequestRepository,
        IHolidayRepository holidayRepository,
        IUnitOfWork unitOfWork,
        PunchPairingService pairingService,
        ILogger<ProcessDailySummariesCommandHandler> logger)
    {
        _punchRepository = punchRepository;
        _summaryRepository = summaryRepository;
        _employeeRepository = employeeRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _holidayRepository = holidayRepository;
        _unitOfWork = unitOfWork;
        _pairingService = pairingService;
        _logger = logger;
    }

    public async Task<ErrorOr<int>> Handle(ProcessDailySummariesCommand command, CancellationToken ct)
    {
        var summaryCount = 0;
        var processedEmployeeDates = new HashSet<(Guid EmployeeId, DateOnly Date)>();

        // Step 1: Process all unprocessed punches grouped by employee + date
        var unprocessedPunches = await _punchRepository
            .GetUnprocessedByDateRangeAsync(command.FromDate, command.ToDate, ct);

        var punchGroups = unprocessedPunches
            .GroupBy(p => (p.EmployeeId, p.PunchDate))
            .ToList();

        foreach (var group in punchGroups)
        {
            var (employeeId, date) = group.Key;
            processedEmployeeDates.Add((employeeId, date));

            try
            {
                await ProcessEmployeeDay(employeeId, date, group.ToList(), ct);
                summaryCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to process attendance for employee {EmployeeId} on {Date}", employeeId, date);
            }
        }

        // Step 2: Fill gaps — employees on approved leave without summaries
        var approvedLeaves = await _leaveRequestRepository
            .GetApprovedForDateRangeAsync(command.FromDate, command.ToDate, ct);

        foreach (var leave in approvedLeaves)
        {
            for (var d = leave.StartDate; d <= leave.EndDate; d = d.AddDays(1))
            {
                if (processedEmployeeDates.Contains((leave.EmployeeId, d)))
                    continue;

                var existing = await _summaryRepository.GetByEmployeeAndDateAsync(leave.EmployeeId, d, ct);
                if (existing != null)
                {
                    existing.MarkAsOnLeave(leave.Id);
                    existing.Calculate();
                    _summaryRepository.Update(existing);
                }
                else
                {
                    var summary = CreateMinimalSummary(leave.EmployeeId, d, leave.Id, ct);
                    _summaryRepository.Add(summary);
                }

                processedEmployeeDates.Add((leave.EmployeeId, d));
                summaryCount++;
            }
        }

        // Step 3: Fill gaps — holidays without summaries
        var holidays = await _holidayRepository.GetByDateRangeAsync(command.FromDate, command.ToDate, ct);

        // For holidays, we need to create summaries for all active employees
        // This is a batch operation; for now, skip bulk generation (handled on demand)
        // The exceptions report will surface any missing holiday summaries

        await _unitOfWork.SaveChangesAsync(ct);
        return summaryCount;
    }

    private async Task ProcessEmployeeDay(
        Guid employeeId, DateOnly date, List<AttendancePunch> punches, CancellationToken ct)
    {
        // Resolve effective shift
        var employee = await _employeeRepository.GetWithDepartmentAndShiftAsync(employeeId, ct);
        if (employee == null)
        {
            _logger.LogWarning("Employee {EmployeeId} not found, skipping", employeeId);
            return;
        }

        var shiftId = employee.ShiftId ?? employee.Department?.DefaultShiftId;
        if (shiftId == null)
        {
            _logger.LogWarning("No shift configured for employee {EmployeeId} on {Date}", employeeId, date);
            return;
        }

        // We need the Shift entity fully loaded for snapshot data
        // This requires a shift repository or query — for now, get via navigation property
        var shift = employee.Shift ?? employee.Department?.DefaultShift;
        if (shift == null)
        {
            _logger.LogWarning("Shift {ShiftId} not loaded for employee {EmployeeId}", shiftId, employeeId);
            return;
        }

        // Pair punches
        var pairing = _pairingService.Pair(punches);

        // Mark consumed punches as processed
        foreach (var punch in punches)
        {
            if (!pairing.OrphanPunches.Contains(punch))
                punch.MarkProcessed();
        }

        // Check for approved leave
        var activeLeaves = await _leaveRequestRepository
            .GetApprovedForEmployeeDateRangeAsync(employeeId, date, date, ct);
        var activeLeave = activeLeaves.FirstOrDefault();

        // Check for holiday
        var holiday = await _holidayRepository.GetByDateAsync(date, ct);

        // Create or update summary
        var existingSummary = await _summaryRepository.GetByEmployeeAndDateAsync(employeeId, date, ct);

        if (existingSummary != null)
        {
            existingSummary.SetPunchData(pairing.FirstPunchIn, pairing.LastPunchOut, pairing.TotalBreakMinutes);
            if (activeLeave != null) existingSummary.MarkAsOnLeave(activeLeave.Id);
            if (holiday != null) existingSummary.MarkAsHoliday(holiday.Id);
            existingSummary.Calculate();
            _summaryRepository.Update(existingSummary);
        }
        else
        {
            var summary = new AttendanceDailySummary(
                employeeId, date,
                shiftId.Value, shift.Name,
                shift.StartTime, shift.EndTime,
                shift.GracePeriodMinutes, shift.LateThresholdMinutes,
                shift.EarlyDepartureThresholdMinutes, shift.OvertimeThresholdMinutes,
                shift.MinimumWorkMinutesForPresence,
                "System");

            summary.SetPunchData(pairing.FirstPunchIn, pairing.LastPunchOut, pairing.TotalBreakMinutes);
            if (activeLeave != null) summary.MarkAsOnLeave(activeLeave.Id);
            if (holiday != null) summary.MarkAsHoliday(holiday.Id);

            // Flag orphan punches in notes
            if (pairing.HasOrphans)
            {
                summary.SetNotes($"Unpaired punches: {pairing.OrphanPunches.Count} punch(es) could not be matched.");
            }

            summary.Calculate();
            _summaryRepository.Add(summary);
        }
    }

    private static AttendanceDailySummary CreateMinimalSummary(
        Guid employeeId, DateOnly date, Guid leaveRequestId, CancellationToken ct)
    {
        var summary = new AttendanceDailySummary(
            employeeId, date,
            Guid.Empty, "(Leave — no shift resolved)",
            new TimeOnly(0, 0), new TimeOnly(0, 0),
            0, 0, 0, 0, 0, "System");

        summary.MarkAsOnLeave(leaveRequestId);
        summary.Calculate();
        return summary;
    }
}
