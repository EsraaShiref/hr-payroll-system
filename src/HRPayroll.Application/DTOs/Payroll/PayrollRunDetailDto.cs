namespace HRPayroll.Application.DTOs.Payroll;

public record PayrollRunDetailDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string EmployeeCode { get; init; } = string.Empty;

    // Status
    public string Status { get; init; } = string.Empty;
    public string? SkipReason { get; init; }
    public string? FailureMessage { get; init; }

    // Attendance
    public int TotalScheduledDays { get; init; }
    public int TotalPresentDays { get; init; }
    public int TotalAbsentDays { get; init; }
    public int TotalLeaveDays { get; init; }
    public int TotalOvertimeMinutes { get; init; }
    public int LateOccurrenceCount { get; init; }
    public int LatePenaltyUnits { get; init; }

    // Earnings
    public decimal BaseSalary { get; init; }
    public decimal TotalAllowances { get; init; }
    public decimal OvertimePay { get; init; }
    public decimal GrossPay { get; init; }

    // Deductions
    public decimal LeaveDeduction { get; init; }
    public decimal LatePenaltyDeduction { get; init; }
    public decimal SocialInsuranceEmployeeShare { get; init; }
    public decimal TaxableIncome { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal TotalDeductions { get; init; }

    // Net
    public decimal NetPay { get; init; }
}
