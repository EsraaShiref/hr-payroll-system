using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRPayroll.Infrastructure.Persistence.Configurations;

public class PayrollRunDetailConfiguration : IEntityTypeConfiguration<PayrollRunDetail>
{
    public void Configure(EntityTypeBuilder<PayrollRunDetail> builder)
    {
        builder.ToTable("PayrollRunDetails");

        builder.HasKey(d => d.Id);

        // Status
        builder.Property(d => d.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(d => d.SkipReason)
            .HasMaxLength(50)
            .HasConversion<string?>();

        builder.Property(d => d.FailureMessage)
            .HasMaxLength(1000);

        // Snapshot values
        builder.Property(d => d.OvertimeRateMultiplier)
            .HasColumnType("decimal(3,2)");

        builder.Property(d => d.StandardDailyHours)
            .HasColumnType("decimal(4,1)");

        // Monetary — earnings
        builder.Property(d => d.BaseSalary).HasColumnType("decimal(18,2)");
        builder.Property(d => d.TotalAllowances).HasColumnType("decimal(18,2)");
        builder.Property(d => d.NonTaxableAllowancesTotal).HasColumnType("decimal(18,2)");
        builder.Property(d => d.OvertimePay).HasColumnType("decimal(18,2)");
        builder.Property(d => d.GrossPay).HasColumnType("decimal(18,2)");

        // Monetary — deductions
        builder.Property(d => d.LeaveDeduction).HasColumnType("decimal(18,2)");
        builder.Property(d => d.LatePenaltyDeduction).HasColumnType("decimal(18,2)");
        builder.Property(d => d.SocialInsuranceEmployeeShare).HasColumnType("decimal(18,2)");
        builder.Property(d => d.SocialInsuranceEmployerShare).HasColumnType("decimal(18,2)");
        builder.Property(d => d.TaxableIncome).HasColumnType("decimal(18,2)");
        builder.Property(d => d.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(d => d.TotalDeductions).HasColumnType("decimal(18,2)");

        // Monetary — net
        builder.Property(d => d.NetPay).HasColumnType("decimal(18,2)");

        // Audit
        builder.Property(d => d.CalculatedBy).HasMaxLength(100);

        builder.Property(d => d.Notes).HasMaxLength(500);

        // Relationships
        builder.HasOne(d => d.PayrollRun)
            .WithMany(r => r.Details)
            .HasForeignKey(d => d.PayrollRunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Employee)
            .WithMany()
            .HasForeignKey(d => d.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(d => new { d.PayrollRunId, d.EmployeeId })
            .IsUnique()
            .HasDatabaseName("IX_PayrollRunDetails_RunId_EmployeeId");

        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
