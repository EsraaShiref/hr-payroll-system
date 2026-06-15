using HRPayroll.Domain.Entities;
using HRPayroll.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRPayroll.Infrastructure.Persistence.Configurations;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("Shifts");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.StartTime).IsRequired();
        builder.Property(s => s.EndTime).IsRequired();
        builder.Property(s => s.GracePeriodMinutes).IsRequired();
        builder.Property(s => s.LateThresholdMinutes).IsRequired();
        builder.Property(s => s.EarlyDepartureThresholdMinutes).IsRequired();
        builder.Property(s => s.OvertimeThresholdMinutes).IsRequired();
        builder.Property(s => s.MinimumWorkMinutesForPresence).IsRequired();
        builder.Property(s => s.MaxBreakMinutes).IsRequired();
        builder.Property(s => s.WorkingDays)
            .IsRequired()
            .HasConversion<int>();

        builder.Ignore(s => s.ScheduledHours);

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
