using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRPayroll.Infrastructure.Persistence.Configurations;

public class PayrollPolicyConfiguration : IEntityTypeConfiguration<PayrollPolicy>
{
    public void Configure(EntityTypeBuilder<PayrollPolicy> builder)
    {
        builder.ToTable("PayrollPolicies");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.EffectiveFrom).IsRequired();

        builder.Property(p => p.WorkingDaysPerMonth).IsRequired();
        builder.Property(p => p.StandardDailyHours).HasColumnType("decimal(4,1)");
        builder.Property(p => p.LateOccurrencesThreshold).IsRequired();
        builder.Property(p => p.DefaultOvertimeRateMultiplier).HasColumnType("decimal(3,2)");
        builder.Property(p => p.CurrencyCode).IsRequired().HasMaxLength(3);

        builder.HasIndex(p => p.EffectiveFrom)
            .HasDatabaseName("IX_PayrollPolicies_EffectiveFrom");

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
