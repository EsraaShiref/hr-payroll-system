using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRPayroll.Infrastructure.Persistence.Configurations;

public class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("LeaveBalances");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.LeaveType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(l => l.Year)
            .IsRequired();

        builder.Property(l => l.TotalDays)
            .IsRequired()
            .HasColumnType("decimal(5,1)");

        builder.Property(l => l.UsedDays)
            .IsRequired()
            .HasColumnType("decimal(5,1)");

        builder.HasOne(l => l.Employee)
            .WithMany()
            .HasForeignKey(l => l.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => new { l.EmployeeId, l.LeaveType, l.Year })
            .IsUnique()
            .HasDatabaseName("IX_LeaveBalances_EmployeeId_LeaveType_Year");

        builder.HasQueryFilter(l => !l.IsDeleted);
    }
}
