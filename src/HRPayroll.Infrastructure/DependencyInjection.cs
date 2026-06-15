using Hangfire;
using HRPayroll.Application.Interfaces;
using HRPayroll.Infrastructure.Persistence;
using HRPayroll.Infrastructure.Persistence.Identity;
using HRPayroll.Infrastructure.Persistence.Interceptors;
using HRPayroll.Infrastructure.Persistence.Repositories;
using HRPayroll.Infrastructure.Services;
using HRPayroll.Infrastructure.Services.FileParsing;
using MediatR;
using Microsoft.AspNetCore.Identity;
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
        services.AddScoped<IAttendancePunchRepository, AttendancePunchRepository>();
        services.AddScoped<IAttendanceSummaryRepository, AttendanceSummaryRepository>();
        services.AddScoped<IHolidayRepository, HolidayRepository>();
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();
        services.AddScoped<IPayrollRepository, PayrollRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Payroll services
        services.AddScoped<IPayrollCalculationService, PayrollCalculationService>();
        services.AddScoped<IPayrollCalculationJob, PayrollCalculationJob>();
        services.AddScoped<IJobScheduler, JobScheduler>();
        services.AddScoped<IPayslipGeneratorService, PayslipGeneratorService>();
        services.AddScoped<IPaymentFileExportService, PaymentFileExportService>();

        // File parsers
        services.AddScoped<CsvParserService>();
        services.AddScoped<ExcelParserService>();
        services.AddScoped<IFileParserService, FileParserService>();

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

        // Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // JWT options
        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

        // Auth services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
