using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRPayroll.Infrastructure.Persistence.Configurations;

public class ContractVersionConfiguration : IEntityTypeConfiguration<ContractVersion>
{
    public void Configure(EntityTypeBuilder<ContractVersion> builder)
    {
        builder.ToTable("ContractVersions");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.VersionNumber)
            .IsRequired();

        builder.OwnsOne(v => v.BaseSalary, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("BaseSalary_Amount")
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName("BaseSalary_Currency")
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("USD");
        });

        builder.Property(v => v.EffectiveFrom)
            .IsRequired();

        builder.Property(v => v.EffectiveTo);

        builder.HasOne(v => v.Contract)
            .WithMany(c => c.Versions)
            .HasForeignKey(v => v.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.AllowanceAssignments)
            .WithOne(aa => aa.ContractVersion)
            .HasForeignKey(aa => aa.ContractVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => v.ContractId)
            .HasDatabaseName("IX_ContractVersions_ContractId");

        builder.HasIndex(v => new { v.ContractId, v.EffectiveFrom })
            .IsDescending(false, true)
            .HasDatabaseName("IX_ContractVersions_ContractId_EffectiveFrom");

        builder.HasQueryFilter(v => !v.IsDeleted);
    }
}
