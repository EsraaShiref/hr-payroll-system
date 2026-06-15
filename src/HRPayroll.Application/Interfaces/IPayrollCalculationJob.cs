namespace HRPayroll.Application.Interfaces;

/// <summary>
/// Hangfire job contract for payroll calculation.
/// Implemented in Infrastructure and registered via AddHangfire.
/// </summary>
public interface IPayrollCalculationJob
{
    Task ProcessPayrollRunAsync(Guid payrollRunId);
}
