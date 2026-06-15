using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRPayroll.Infrastructure.Persistence.Configurations;

public class AttendanceDailySummaryConfiguration : IEntityTypeConfiguration<AttendanceDailySummary>
{
    public void Configure(EntityTypeBuilder<AttendanceDailySummary> builder)
    {
        builder.ToTable("AttendanceDailySummaries");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Date).IsRequired();

        // Shift snapshot fields
        builder.Property(s => s.ShiftName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.ScheduledStart).IsRequired();
        builder.Property(s => s.ScheduledEnd).IsRequired();

        // Calculated outputs
        builder.Property(s => s.TotalWorkedMinutes).IsRequired();
        builder.Property(s => s.LateMinutes).IsRequired();
        builder.Property(s => s.EarlyDepartureMinutes).IsRequired();
        builder.Property(s => s.OvertimeMinutes).IsRequired();

        // Flags
        builder.Property(s => s.MinimumWorkMinutesForPresence).IsRequired();

        // Override
        builder.Property(s => s.OverrideReason).HasMaxLength(500);

        // Audit
        builder.Property(s => s.CalculatedBy).IsRequired().HasMaxLength(100);

        // Status is NOT mapped — it's a computed (derived) property
        builder.Ignore(s => s.Status);
        builder.Ignore(s => s.NetWorkedMinutes);

        builder.HasOne(s => s.Employee)
            .WithMany()
            .HasForeignKey(s => s.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.EmployeeId, s.Date })
            .IsUnique()
            .HasDatabaseName("IX_AttendanceDailySummaries_EmployeeId_Date");

        builder.HasIndex(s => s.Date)
            .HasDatabaseName("IX_AttendanceDailySummaries_Date");

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
