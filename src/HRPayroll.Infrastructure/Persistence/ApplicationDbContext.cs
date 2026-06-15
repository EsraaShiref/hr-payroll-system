using HRPayroll.Application.Interfaces;
using HRPayroll.Domain.Entities;
using HRPayroll.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace HRPayroll.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
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

    IQueryable<Employee> IApplicationDbContext.Employees => Employees;
    IQueryable<Department> IApplicationDbContext.Departments => Departments;
    IQueryable<Position> IApplicationDbContext.Positions => Positions;
    IQueryable<Contract> IApplicationDbContext.Contracts => Contracts;
    IQueryable<ContractVersion> IApplicationDbContext.ContractVersions => ContractVersions;
    IQueryable<Allowance> IApplicationDbContext.Allowances => Allowances;
    IQueryable<AllowanceAssignment> IApplicationDbContext.AllowanceAssignments => AllowanceAssignments;
    IQueryable<TaxBracketSet> IApplicationDbContext.TaxBracketSets => TaxBracketSets;
    IQueryable<SocialInsuranceConfig> IApplicationDbContext.SocialInsuranceConfigs => SocialInsuranceConfigs;

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
