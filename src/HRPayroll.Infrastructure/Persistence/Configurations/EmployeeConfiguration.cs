using HRPayroll.Domain.Entities;
using HRPayroll.Infrastructure.Persistence.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRPayroll.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EmployeeCode)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<EmployeeCodeConverter>();

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.MiddleName)
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.DateOfBirth)
            .IsRequired();

        builder.Property(e => e.Gender)
            .IsRequired()
            .HasMaxLength(10)
            .HasConversion<string>();

        builder.Property(e => e.NationalId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.PersonalEmail)
            .HasMaxLength(200);

        builder.Property(e => e.PendingNewEmail)
            .HasMaxLength(200);

        builder.Property(e => e.IsEmailChangePending)
            .IsRequired();

        builder.Property(e => e.PhoneNumber)
            .HasMaxLength(30);

        builder.OwnsOne(e => e.Address, addr =>
        {
            addr.Property(a => a.Street).HasColumnName("Street").HasMaxLength(300);
            addr.Property(a => a.City).HasColumnName("City").HasMaxLength(100);
            addr.Property(a => a.State).HasColumnName("State").HasMaxLength(100);
            addr.Property(a => a.PostalCode).HasColumnName("PostalCode").HasMaxLength(20);
            addr.Property(a => a.Country).HasColumnName("Country").HasMaxLength(100);
        });

        builder.Property(e => e.EmergencyContactName)
            .HasMaxLength(200);

        builder.Property(e => e.EmergencyContactPhone)
            .HasMaxLength(30);

        builder.Property(e => e.EmploymentStatus)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(e => e.HireDate)
            .IsRequired();

        builder.Property(e => e.TerminationReason)
            .HasMaxLength(500);

        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Position)
            .WithMany(p => p.Employees)
            .HasForeignKey(e => e.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Shift)
            .WithMany()
            .HasForeignKey(e => e.ShiftId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Contracts)
            .WithOne(c => c.Employee)
            .HasForeignKey(c => c.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.EmployeeCode)
            .IsUnique()
            .HasDatabaseName("IX_Employees_EmployeeCode");

        builder.HasIndex(e => e.NationalId)
            .IsUnique()
            .HasDatabaseName("IX_Employees_NationalId");

        builder.HasIndex(e => e.DepartmentId)
            .HasDatabaseName("IX_Employees_DepartmentId");

        builder.HasIndex(e => e.PositionId)
            .HasDatabaseName("IX_Employees_PositionId");

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
