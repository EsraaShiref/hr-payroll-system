namespace HRPayroll.Application.Interfaces;

/// <summary>
/// Abstraction over the background job scheduler (Hangfire).
/// Keeps the Application layer free of Hangfire dependency.
/// </summary>
public interface IJobScheduler
{
    void EnqueuePayrollRun(Guid payrollRunId);
    void EnqueuePayslipGeneration(Guid payrollRunId);
}
