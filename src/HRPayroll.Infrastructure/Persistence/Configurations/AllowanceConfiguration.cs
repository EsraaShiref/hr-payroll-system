using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRPayroll.Infrastructure.Persistence.Configurations;

public class AllowanceConfiguration : IEntityTypeConfiguration<Allowance>
{
    public void Configure(EntityTypeBuilder<Allowance> builder)
    {
        builder.ToTable("Allowances");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Description)
            .HasMaxLength(500);

        builder.Property(a => a.Type)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(a => a.DefaultAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.DefaultPercentage)
            .HasColumnType("decimal(5,2)");

        builder.Property(a => a.Taxability)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.HasIndex(a => a.Code)
            .IsUnique()
            .HasDatabaseName("IX_Allowances_Code");

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
