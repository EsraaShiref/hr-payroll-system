using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRPayroll.Infrastructure.Persistence.Configurations;

public class SocialInsuranceConfigConfiguration : IEntityTypeConfiguration<SocialInsuranceConfig>
{
    public void Configure(EntityTypeBuilder<SocialInsuranceConfig> builder)
    {
        builder.ToTable("SocialInsuranceConfigs");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.EmployeeContributionPercent)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.Property(s => s.EmployerContributionPercent)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.Property(s => s.MaxContributionSalary)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.EffectiveFrom)
            .IsRequired();

        builder.Property(s => s.EffectiveTo);

        builder.HasIndex(s => new { s.EffectiveFrom, s.EffectiveTo })
            .HasDatabaseName("IX_SocialInsuranceConfigs_EffectiveRange");

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
