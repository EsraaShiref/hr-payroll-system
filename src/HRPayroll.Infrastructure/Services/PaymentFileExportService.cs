using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;

namespace HRPayroll.Infrastructure.Services;

public class PaymentFileExportService : IPaymentFileExportService
{
    public Task<byte[]> GenerateBankTransferCsvAsync(IEnumerable<PayrollRunDetail> details, CancellationToken ct = default)
    {
        // TODO: Implement CSV generation in Module D Sub-step 5
        throw new NotImplementedException("Payment file export not yet implemented.");
    }
}
