using Hangfire;
using HRPayroll.Application.Interfaces;
using HRPayroll.Infrastructure.Persistence;
using HRPayroll.Infrastructure.Persistence.Interceptors;
using HRPayroll.Infrastructure.Persistence.Repositories;
using HRPayroll.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HRPayroll.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection string is not configured.");

        // Audit interceptor (scoped because ICurrentUserService is scoped)
        services.AddScoped<AuditInterceptor>();

        // DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // DbContext abstraction for query handlers
        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Current user
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();

        // MediatR — scans Infrastructure assembly for query handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        // Hangfire storage (configured but jobs activated later)
        services.AddHangfire(config =>
            config.UseSqlServerStorage(connectionString));

        return services;
    }
}
