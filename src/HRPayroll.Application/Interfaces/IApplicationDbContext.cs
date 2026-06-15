using HRPayroll.Domain.Entities;

namespace HRPayroll.Application.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<Employee> Employees { get; }
    IQueryable<Department> Departments { get; }
    IQueryable<Position> Positions { get; }
    IQueryable<Contract> Contracts { get; }
    IQueryable<ContractVersion> ContractVersions { get; }
    IQueryable<Allowance> Allowances { get; }
    IQueryable<AllowanceAssignment> AllowanceAssignments { get; }
    IQueryable<TaxBracketSet> TaxBracketSets { get; }
    IQueryable<SocialInsuranceConfig> SocialInsuranceConfigs { get; }
    IQueryable<AttendancePunch> AttendancePunches { get; }
    IQueryable<AttendanceDailySummary> AttendanceDailySummaries { get; }
    IQueryable<Shift> Shifts { get; }
    IQueryable<Holiday> Holidays { get; }
    IQueryable<LeaveRequest> LeaveRequests { get; }
    IQueryable<LeaveBalance> LeaveBalances { get; }
}
