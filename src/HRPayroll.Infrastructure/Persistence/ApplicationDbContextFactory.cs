using HRPayroll.Application.Interfaces;
using HRPayroll.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HRPayroll.Infrastructure.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=HRPayrollDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        var currentUserService = new DesignTimeCurrentUserService();
        var auditInterceptor = new AuditInterceptor(currentUserService);

        return new ApplicationDbContext(optionsBuilder.Options, auditInterceptor);
    }

    private class DesignTimeCurrentUserService : ICurrentUserService
    {
        public string UserId => "migration";
        public string? UserName => "Migration Tool";
        public string[] Roles => Array.Empty<string>();
        public string[] Claims => Array.Empty<string>();
        public Guid? EmployeeId => null;
    }
}
