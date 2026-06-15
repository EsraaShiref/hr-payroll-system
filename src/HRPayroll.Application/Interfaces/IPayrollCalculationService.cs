using HRPayroll.Domain.Entities;

namespace HRPayroll.Application.Interfaces;

/// <summary>
/// Pure computation service for per-employee payroll calculation.
/// All data is passed in via PayrollCalculationRequest — no DB queries inside.
/// Returns a fully populated PayrollRunDetail.
/// </summary>
public interface IPayrollCalculationService
{
    PayrollRunDetail Calculate(PayrollCalculationRequest request);
}
