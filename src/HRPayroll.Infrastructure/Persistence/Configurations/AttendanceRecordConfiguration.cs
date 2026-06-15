using HRPayroll.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRPayroll.Infrastructure.Persistence.Configurations;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Date)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(a => a.Notes)
            .HasMaxLength(500);

        builder.HasOne(a => a.Employee)
            .WithMany()
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.EmployeeId, a.Date })
            .IsUnique()
            .HasDatabaseName("IX_AttendanceRecords_EmployeeId_Date");

        builder.HasIndex(a => a.Date)
            .HasDatabaseName("IX_AttendanceRecords_Date");

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
