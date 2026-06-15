using HRPayroll.Domain.Entities;

namespace HRPayroll.Application.Interfaces;

public interface IPayslipGeneratorService
{
    Task<byte[]> GeneratePayslipPdfAsync(PayrollRunDetail detail, CancellationToken ct = default);
}
