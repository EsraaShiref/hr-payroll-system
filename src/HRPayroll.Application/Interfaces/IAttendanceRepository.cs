using HRPayroll.Domain.Entities;

namespace HRPayroll.Application.Interfaces;

public interface IAttendanceRepository
{
    Task<AttendanceRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AttendanceRecord?> GetByEmployeeAndDateAsync(Guid employeeId, DateOnly date, CancellationToken ct = default);
    Task<List<AttendanceRecord>> GetByEmployeeDateRangeAsync(Guid employeeId, DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<List<AttendanceRecord>> GetByDateAsync(DateOnly date, CancellationToken ct = default);
    void Add(AttendanceRecord record);
    void Update(AttendanceRecord record);
}

public interface ILeaveRequestRepository
{
    Task<LeaveRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<LeaveRequest>> GetPendingByDepartmentAsync(Guid departmentId, CancellationToken ct = default);
    Task<List<LeaveRequest>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
    void Add(LeaveRequest leaveRequest);
    void Update(LeaveRequest leaveRequest);
}

public interface ILeaveBalanceRepository
{
    Task<LeaveBalance?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<LeaveBalance?> GetByEmployeeAndTypeYearAsync(Guid employeeId, Domain.Enums.LeaveType leaveType, int year, CancellationToken ct = default);
    Task<List<LeaveBalance>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
    void Add(LeaveBalance leaveBalance);
    void Update(LeaveBalance leaveBalance);
}
