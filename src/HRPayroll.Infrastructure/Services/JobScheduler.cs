using HRPayroll.Application.Interfaces;
using Hangfire;

namespace HRPayroll.Infrastructure.Services;

public class JobScheduler : IJobScheduler
{
    private readonly IBackgroundJobClient _backgroundJobs;

    public JobScheduler(IBackgroundJobClient backgroundJobs)
    {
        _backgroundJobs = backgroundJobs;
    }

    public void EnqueuePayrollRun(Guid payrollRunId)
    {
        _backgroundJobs.Enqueue<IPayrollCalculationJob>(job =>
            job.ProcessPayrollRunAsync(payrollRunId));
    }

    public void EnqueuePayslipGeneration(Guid payrollRunId)
    {
        // Payslip generation deferred — implemented in a future sub-step
    }
}
