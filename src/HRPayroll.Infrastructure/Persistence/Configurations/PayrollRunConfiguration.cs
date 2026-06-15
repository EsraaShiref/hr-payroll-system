using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRPayroll.Infrastructure.Persistence.Configurations;

public class PayrollRunConfiguration : IEntityTypeConfiguration<PayrollRun>
{
    public void Configure(EntityTypeBuilder<PayrollRun> builder)
    {
        builder.ToTable("PayrollRuns");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Year).IsRequired();
        builder.Property(r => r.Month).IsRequired();

        builder.Property(r => r.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(r => r.RowVersion)
            .IsRowVersion();

        builder.Property(r => r.StartedAt);
        builder.Property(r => r.CompletedAt);

        builder.Property(r => r.ProcessedBy)
            .HasMaxLength(100);

        builder.Property(r => r.Notes)
            .HasMaxLength(1000);

        builder.HasMany(r => r.Details)
            .WithOne(d => d.PayrollRun)
            .HasForeignKey(d => d.PayrollRunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.Year, r.Month })
            .HasDatabaseName("IX_PayrollRuns_Year_Month");

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
