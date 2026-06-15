using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRPayroll.Infrastructure.Persistence.Configurations;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("Contracts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ContractType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(c => c.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(c => c.SignedDate)
            .IsRequired();

        builder.Property(c => c.TerminationReason)
            .HasMaxLength(500);

        builder.HasOne(c => c.Employee)
            .WithMany(e => e.Contracts)
            .HasForeignKey(c => c.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Versions)
            .WithOne(v => v.Contract)
            .HasForeignKey(v => v.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.EmployeeId)
            .HasDatabaseName("IX_Contracts_EmployeeId");

        builder.HasIndex(c => new { c.EmployeeId, c.Status })
            .HasDatabaseName("IX_Contracts_EmployeeId_Status");

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
