using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRPayroll.Infrastructure.Persistence.Configurations;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.LeaveType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(l => l.StartDate)
            .IsRequired();

        builder.Property(l => l.EndDate)
            .IsRequired();

        builder.Property(l => l.TotalDays)
            .IsRequired()
            .HasColumnType("decimal(5,1)");

        builder.Property(l => l.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(l => l.Reason)
            .HasMaxLength(500);

        builder.Property(l => l.RejectionReason)
            .HasMaxLength(500);

        builder.HasOne(l => l.Employee)
            .WithMany()
            .HasForeignKey(l => l.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.ApprovedBy)
            .WithMany()
            .HasForeignKey(l => l.ApprovedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.Status)
            .HasDatabaseName("IX_LeaveRequests_Status");

        builder.HasIndex(l => l.EmployeeId)
            .HasDatabaseName("IX_LeaveRequests_EmployeeId");

        builder.HasQueryFilter(l => !l.IsDeleted);
    }
}
