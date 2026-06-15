using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRPayroll.Infrastructure.Persistence.Configurations;

public class AllowanceAssignmentConfiguration : IEntityTypeConfiguration<AllowanceAssignment>
{
    public void Configure(EntityTypeBuilder<AllowanceAssignment> builder)
    {
        builder.ToTable("AllowanceAssignments");

        builder.HasKey(aa => aa.Id);

        builder.Property(aa => aa.OverrideAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(aa => aa.OverridePercentage)
            .HasColumnType("decimal(5,2)");

        builder.HasOne(aa => aa.ContractVersion)
            .WithMany(v => v.AllowanceAssignments)
            .HasForeignKey(aa => aa.ContractVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(aa => aa.Allowance)
            .WithMany()
            .HasForeignKey(aa => aa.AllowanceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(aa => aa.ContractVersionId)
            .HasDatabaseName("IX_AllowanceAssignments_ContractVersionId");

        builder.HasIndex(aa => aa.AllowanceId)
            .HasDatabaseName("IX_AllowanceAssignments_AllowanceId");

        builder.HasQueryFilter(aa => !aa.IsDeleted);
    }
}
