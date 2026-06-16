using HRPayroll.Application.DTOs.Payroll;
using HRPayroll.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HRPayroll.Infrastructure.Services;

public class PayslipGeneratorService : IPayslipGeneratorService
{
    static PayslipGeneratorService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GeneratePayslipPdfAsync(PayslipData data, CancellationToken ct = default)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(c => ComposeHeader(c, data));
                page.Content().Element(c => ComposeBody(c, data));
                page.Footer().Element(c => ComposeFooter(c, data));
            });
        });

        var pdf = doc.GeneratePdf();
        return Task.FromResult(pdf);
    }

    private static void ComposeHeader(IContainer c, PayslipData data)
    {
        c.Column(col =>
        {
            col.Item().AlignCenter().Text(data.CompanyName)
                .Bold().FontSize(18).FontColor(Colors.Blue.Darken3);

            col.Item().AlignCenter().Text("PAYSLIP")
                .Bold().FontSize(14).FontColor(Colors.Blue.Darken2);

            col.Item().AlignCenter().Text(data.PeriodLabel)
                .FontSize(11).FontColor(Colors.Grey.Darken2);

            col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
        });
    }

    private static void ComposeBody(IContainer c, PayslipData data)
    {
        c.Column(col =>
        {
            col.Item().PaddingTop(12).Element(e => ComposeEmployeeSection(e, data));
            col.Item().PaddingTop(12).Element(e => ComposeTables(e, data));
            col.Item().PaddingTop(12).Element(e => ComposeNetPay(e, data));
        });
    }

    private static void ComposeEmployeeSection(IContainer c, PayslipData data)
    {
        c.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn();
                cols.RelativeColumn();
                cols.RelativeColumn();
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Employee")
                    .Bold().FontSize(9);
                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Details")
                    .Bold().FontSize(9);
                header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Attendance")
                    .Bold().FontSize(9);
            });

            table.Cell().Padding(2).Text($"Name:").SemiBold().FontSize(9);
            table.Cell().Padding(2).Text(data.EmployeeName).FontSize(9);
            table.Cell().Padding(2).Text($"Present: {data.PresentDays}").FontSize(9);

            table.Cell().Padding(2).Text($"Code:").SemiBold().FontSize(9);
            table.Cell().Padding(2).Text(data.EmployeeCode).FontSize(9);
            table.Cell().Padding(2).Text($"Absent: {data.AbsentDays}").FontSize(9);

            table.Cell().Padding(2).Text($"Department:").SemiBold().FontSize(9);
            table.Cell().Padding(2).Text(data.Department).FontSize(9);
            table.Cell().Padding(2).Text($"Leave: {data.LeaveDays}").FontSize(9);

            table.Cell().Padding(2).Text($"Position:").SemiBold().FontSize(9);
            table.Cell().Padding(2).Text(data.Position).FontSize(9);
            table.Cell().Padding(2).Text($"Late: {data.LateOccurrences}").FontSize(9);

            table.Cell().Padding(2).Text($"Contract:").SemiBold().FontSize(9);
            table.Cell().Padding(2).Text(data.ContractType).FontSize(9);
            table.Cell().Padding(2).Text($"OT: {data.OvertimeHours:F1}h").FontSize(9);
        });
    }

    private static void ComposeTables(IContainer c, PayslipData data)
    {
        c.Row(row =>
        {
            row.RelativeItem(1).Element(e => ComposeEarningsTable(e, data));
            row.ConstantItem(20);
            row.RelativeItem(1).Element(e => ComposeDeductionsTable(e, data));
        });
    }

    private static void ComposeEarningsTable(IContainer c, PayslipData data)
    {
        c.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(2);
                cols.RelativeColumn(1);
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Green.Lighten3).Padding(4).Text("Earnings")
                    .Bold().FontSize(9);
                header.Cell().Background(Colors.Green.Lighten3).Padding(4).AlignRight().Text("Amount")
                    .Bold().FontSize(9);
            });

            foreach (var line in data.Earnings)
            {
                table.Cell().Padding(2).Text(line.Label).FontSize(9);
                table.Cell().Padding(2).AlignRight().Text($"{line.Amount:N2}").FontSize(9);
            }

            table.Cell().Padding(4).Background(Colors.Green.Lighten4).Text("Gross Pay")
                .Bold().FontSize(9);
            table.Cell().Padding(4).Background(Colors.Green.Lighten4).AlignRight()
                .Text($"{data.GrossPay:N2}").Bold().FontSize(9);
        });
    }

    private static void ComposeDeductionsTable(IContainer c, PayslipData data)
    {
        c.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(2);
                cols.RelativeColumn(1);
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Red.Lighten3).Padding(4).Text("Deductions")
                    .Bold().FontSize(9);
                header.Cell().Background(Colors.Red.Lighten3).Padding(4).AlignRight().Text("Amount")
                    .Bold().FontSize(9);
            });

            foreach (var line in data.Deductions)
            {
                table.Cell().Padding(2).Text(line.Label).FontSize(9);
                table.Cell().Padding(2).AlignRight().Text($"{line.Amount:N2}").FontSize(9);
            }

            table.Cell().Padding(4).Background(Colors.Red.Lighten4).Text("Total Deductions")
                .Bold().FontSize(9);
            table.Cell().Padding(4).Background(Colors.Red.Lighten4).AlignRight()
                .Text($"{data.TotalDeductions:N2}").Bold().FontSize(9);
        });
    }

    private static void ComposeNetPay(IContainer c, PayslipData data)
    {
        c.Border(1.5f).BorderColor(Colors.Blue.Darken3).Padding(12)
            .Background(Colors.Blue.Lighten5).Column(col =>
            {
                col.Item().AlignCenter().Text("NET PAY").Bold().FontSize(14)
                    .FontColor(Colors.Blue.Darken3);
                col.Item().AlignCenter().Text($"{data.NetPay:N2}").Bold().FontSize(22)
                    .FontColor(Colors.Blue.Darken3);
            });
    }

    private static void ComposeFooter(IContainer c, PayslipData data)
    {
        c.Column(col =>
        {
            col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
            col.Item().PaddingTop(4).AlignCenter().Text(
                "This is a system-generated document").Italic().FontSize(8).FontColor(Colors.Grey.Darken1);
            col.Item().AlignCenter().Text(
                $"Generated: {data.CalculatedAt:yyyy-MM-dd HH:mm}  |  Run ID: {data.PayrollRunId}")
                .FontSize(8).FontColor(Colors.Grey.Darken1);
        });
    }
}
