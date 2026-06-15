using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;

namespace HRPayroll.Infrastructure.Services;

public class PayslipGeneratorService : IPayslipGeneratorService
{
    public Task<byte[]> GeneratePayslipPdfAsync(PayrollRunDetail detail, CancellationToken ct = default)
    {
        // TODO: Implement PDF generation in Module D Sub-step 5
        throw new NotImplementedException("Payslip PDF generation not yet implemented.");
    }
}
