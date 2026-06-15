using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRPayroll.Infrastructure.Persistence.Configurations;

public class AttendancePunchConfiguration : IEntityTypeConfiguration<AttendancePunch>
{
    public void Configure(EntityTypeBuilder<AttendancePunch> builder)
    {
        builder.ToTable("AttendancePunches");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.TimestampUtc)
            .IsRequired();

        builder.Property(p => p.Type)
            .IsRequired()
            .HasMaxLength(10)
            .HasConversion<string>();

        builder.Property(p => p.Source)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(p => p.DeviceId)
            .HasMaxLength(100);

        builder.Property(p => p.Notes)
            .HasMaxLength(500);

        builder.HasOne(p => p.Employee)
            .WithMany()
            .HasForeignKey(p => p.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.EmployeeId, p.TimestampUtc, p.Type, p.DeviceId })
            .IsUnique()
            .HasDatabaseName("IX_AttendancePunches_Unique")
            .HasFilter(null);

        builder.HasIndex(p => p.TimestampUtc)
            .HasDatabaseName("IX_AttendancePunches_Timestamp");

        builder.HasIndex(p => p.IsProcessed)
            .HasDatabaseName("IX_AttendancePunches_IsProcessed")
            .HasFilter("[IsProcessed] = 0");

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
