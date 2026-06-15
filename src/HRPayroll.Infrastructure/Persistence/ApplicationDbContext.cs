using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using HRPayroll.Infrastructure.Persistence.Identity;
using HRPayroll.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>, IApplicationDbContext
{
    private readonly AuditInterceptor _auditInterceptor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        AuditInterceptor auditInterceptor)
        : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractVersion> ContractVersions => Set<ContractVersion>();
    public DbSet<Allowance> Allowances => Set<Allowance>();
    public DbSet<AllowanceAssignment> AllowanceAssignments => Set<AllowanceAssignment>();
    public DbSet<TaxBracketSet> TaxBracketSets => Set<TaxBracketSet>();
    public DbSet<SocialInsuranceConfig> SocialInsuranceConfigs => Set<SocialInsuranceConfig>();
    public DbSet<AttendancePunch> AttendancePunches => Set<AttendancePunch>();
    public DbSet<AttendanceDailySummary> AttendanceDailySummaries => Set<AttendanceDailySummary>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();

    IQueryable<Employee> IApplicationDbContext.Employees => Employees;
    IQueryable<Department> IApplicationDbContext.Departments => Departments;
    IQueryable<Position> IApplicationDbContext.Positions => Positions;
    IQueryable<Contract> IApplicationDbContext.Contracts => Contracts;
    IQueryable<ContractVersion> IApplicationDbContext.ContractVersions => ContractVersions;
    IQueryable<Allowance> IApplicationDbContext.Allowances => Allowances;
    IQueryable<AllowanceAssignment> IApplicationDbContext.AllowanceAssignments => AllowanceAssignments;
    IQueryable<TaxBracketSet> IApplicationDbContext.TaxBracketSets => TaxBracketSets;
    IQueryable<SocialInsuranceConfig> IApplicationDbContext.SocialInsuranceConfigs => SocialInsuranceConfigs;
    IQueryable<AttendancePunch> IApplicationDbContext.AttendancePunches => AttendancePunches;
    IQueryable<AttendanceDailySummary> IApplicationDbContext.AttendanceDailySummaries => AttendanceDailySummaries;
    IQueryable<Shift> IApplicationDbContext.Shifts => Shifts;
    IQueryable<Holiday> IApplicationDbContext.Holidays => Holidays;
    IQueryable<LeaveRequest> IApplicationDbContext.LeaveRequests => LeaveRequests;
    IQueryable<LeaveBalance> IApplicationDbContext.LeaveBalances => LeaveBalances;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Rename Identity tables to match project conventions
        modelBuilder.Entity<ApplicationUser>(entity => entity.ToTable("Users"));
        modelBuilder.Entity<IdentityRole>(entity => entity.ToTable("Roles"));
        modelBuilder.Entity<IdentityUserRole<string>>(entity => entity.ToTable("UserRoles"));
        modelBuilder.Entity<IdentityUserClaim<string>>(entity => entity.ToTable("UserClaims"));
        modelBuilder.Entity<IdentityUserLogin<string>>(entity => entity.ToTable("UserLogins"));
        modelBuilder.Entity<IdentityUserToken<string>>(entity => entity.ToTable("UserTokens"));
        modelBuilder.Entity<IdentityRoleClaim<string>>(entity => entity.ToTable("RoleClaims"));
    }
}
