using HRPayroll.Domain.Entities;
using HRPayroll.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
