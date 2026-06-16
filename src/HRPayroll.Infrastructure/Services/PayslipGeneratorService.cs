using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;

namespace HRPayroll.Infrastructure.Services;

public class PayslipGeneratorService : IPayslipGeneratorService
{
    public Task<byte[]> GeneratePayslipPdfAsync(PayrollRunDetail detail, CancellationToken ct = default)
    {
        // TODO: Implement PDF generation in Module D Sub-step 5
        // Return placeholder content so the download button doesn't fail silently
        var placeholder = System.Text.Encoding.UTF8.GetBytes(
            $"Payslip for employee {detail.EmployeeId} — not yet implemented.");
        return Task.FromResult(placeholder);
    }
}
