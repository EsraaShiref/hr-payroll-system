namespace HRPayroll.Application.DTOs.Dashboard;

public record DashboardAttendanceSummaryDto
{
    public int TotalPresent { get; init; }
    public int TotalAbsent { get; init; }
    public int TotalLate { get; init; }
    public int TotalOnLeave { get; init; }
    public int TotalHoliday { get; init; }
    public int TotalPendingReview { get; init; }
    public int TotalActiveEmployees { get; init; }
    public List<DepartmentAttendanceDto> DepartmentBreakdown { get; init; } = new();
}

public record DepartmentAttendanceDto
{
    public string DepartmentName { get; init; } = string.Empty;
    public int Present { get; init; }
    public int Absent { get; init; }
    public int Late { get; init; }
    public int OnLeave { get; init; }
    public int Total { get; init; }
}

public record PendingLeaveRequestDto
{
    public Guid LeaveRequestId { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string LeaveType { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public decimal TotalDays { get; init; }
    public string? Reason { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record PayrollBudgetSummaryDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal ProjectedGrossPay { get; init; }
    public decimal ActualNetPay { get; init; }
    public decimal ActualGrossPay { get; init; }
    public decimal ActualTotalDeductions { get; init; }
    public int ActualEmployeeCount { get; init; }
    public List<DepartmentBudgetDto> DepartmentBreakdown { get; init; } = new();
}

public record DepartmentBudgetDto
{
    public string DepartmentName { get; init; } = string.Empty;
    public decimal ProjectedBaseSalaries { get; init; }
    public decimal ActualNetPay { get; init; }
    public int EmployeeCount { get; init; }
}

public record HeadcountTrendDto
{
    public List<MonthlyHeadcountDto> Months { get; init; } = new();
    public int CurrentHeadcount { get; init; }
}

public record MonthlyHeadcountDto
{
    public string Label { get; init; } = string.Empty;
    public int Year { get; init; }
    public int Month { get; init; }
    public int Count { get; init; }
    public int ChangeFromPrevious { get; init; }
}

public record UpcomingContractRenewalDto
{
    public Guid ContractId { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string ContractType { get; init; } = string.Empty;
    public DateOnly ExpiryDate { get; init; }
    public int DaysRemaining { get; init; }
}
