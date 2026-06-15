using HRPayroll.Domain.Entities;

namespace HRPayroll.Application.Interfaces;

public interface IPaymentFileExportService
{
    Task<byte[]> GenerateBankTransferCsvAsync(IEnumerable<PayrollRunDetail> details, CancellationToken ct = default);
}
