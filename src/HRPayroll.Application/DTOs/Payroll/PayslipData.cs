namespace HRPayroll.Application.DTOs.Payroll;

public record PayslipEarningLine(string Label, decimal Amount);
public record PayslipDeductionLine(string Label, decimal Amount);

public record PayslipData
{
    public string CompanyName { get; init; } = "Company Name";
    public string PeriodLabel { get; init; } = string.Empty;

    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string EmployeeCode { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Position { get; init; } = string.Empty;
    public string ContractType { get; init; } = string.Empty;

    public int PresentDays { get; init; }
    public int AbsentDays { get; init; }
    public int LeaveDays { get; init; }
    public int LateOccurrences { get; init; }
    public decimal OvertimeHours { get; init; }

    public List<PayslipEarningLine> Earnings { get; init; } = new();
    public decimal GrossPay { get; init; }

    public List<PayslipDeductionLine> Deductions { get; init; } = new();
    public decimal TotalDeductions { get; init; }

    public decimal NetPay { get; init; }

    public DateTime CalculatedAt { get; init; }
    public Guid PayrollRunId { get; init; }
}
