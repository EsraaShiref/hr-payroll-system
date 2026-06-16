using HRPayroll.Domain.Entities;
using HRPayroll.Domain.Enums;

namespace HRPayroll.Application.Interfaces;

public interface IAttendancePunchRepository
{
    Task<AttendancePunch?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<AttendancePunch>> GetUnprocessedByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<List<AttendancePunch>> GetByEmployeeAndDateAsync(Guid employeeId, DateOnly date, CancellationToken ct = default);
    Task<bool> ExistsDuplicateAsync(Guid employeeId, DateTime timestampUtc, PunchType type, string? deviceId, CancellationToken ct = default);
    void Add(AttendancePunch punch);
    void AddRange(IEnumerable<AttendancePunch> punches);
    void Update(AttendancePunch punch);
}

public interface IAttendanceSummaryRepository
{
    Task<AttendanceDailySummary?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AttendanceDailySummary?> GetByEmployeeAndDateAsync(Guid employeeId, DateOnly date, CancellationToken ct = default);
    Task<List<AttendanceDailySummary>> GetByEmployeeDateRangeAsync(Guid employeeId, DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<List<AttendanceDailySummary>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    void Add(AttendanceDailySummary summary);
    void Update(AttendanceDailySummary summary);
}

public interface ILeaveRequestRepository
{
    Task<LeaveRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<LeaveRequest>> GetPendingByDepartmentAsync(Guid departmentId, CancellationToken ct = default);
    Task<List<LeaveRequest>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
    Task<List<LeaveRequest>> GetApprovedForDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<List<LeaveRequest>> GetApprovedForEmployeeDateRangeAsync(Guid employeeId, DateOnly from, DateOnly to, CancellationToken ct = default);
    void Add(LeaveRequest leaveRequest);
    void Update(LeaveRequest leaveRequest);
}

public interface ILeaveBalanceRepository
{
    Task<LeaveBalance?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<LeaveBalance?> GetByEmployeeAndTypeYearAsync(Guid employeeId, LeaveType leaveType, int year, CancellationToken ct = default);
    Task<List<LeaveBalance>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
    void Add(LeaveBalance leaveBalance);
    void Update(LeaveBalance leaveBalance);
}

public interface IHolidayRepository
{
    Task<Holiday?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Holiday>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<Holiday?> GetByDateAsync(DateOnly date, CancellationToken ct = default);
    void Add(Holiday holiday);
    void Update(Holiday holiday);
    void Remove(Holiday holiday);
}
