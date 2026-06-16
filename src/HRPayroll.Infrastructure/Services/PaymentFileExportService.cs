using System.Text;
using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;

namespace HRPayroll.Infrastructure.Services;

public class PaymentFileExportService : IPaymentFileExportService
{
    public Task<byte[]> GenerateBankTransferCsvAsync(IEnumerable<PayrollRunDetail> details, CancellationToken ct = default)
    {
        // TODO: Implement full CSV generation in Module D Sub-step 5
        var header = "EmployeeCode,EmployeeName,BankAccountNumber,BankName,NetPay,Currency";
        var rows = new List<string> { header };

        foreach (var detail in details)
        {
            var employee = detail.Employee;
            rows.Add($"{employee?.EmployeeCode.Value ?? ""},{employee?.FirstName + " " + employee?.LastName ?? ""},,,{detail.NetPay:F2},USD");
        }

        return Task.FromResult(Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, rows)));
    }
}
