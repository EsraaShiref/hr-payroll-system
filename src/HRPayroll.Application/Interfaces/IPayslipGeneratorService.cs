using HRPayroll.Application.DTOs.Payroll;

namespace HRPayroll.Application.Interfaces;

public interface IPayslipGeneratorService
{
    Task<byte[]> GeneratePayslipPdfAsync(PayslipData data, CancellationToken ct = default);
}
