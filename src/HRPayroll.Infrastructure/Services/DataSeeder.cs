using HRPayroll.Domain.Entities;
using HRPayroll.Domain.Enums;
using HRPayroll.Domain.ValueObjects;
using HRPayroll.Infrastructure.Persistence;
using HRPayroll.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRPayroll.Infrastructure.Services;

public sealed class DataSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<DataSeeder> logger)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (await _db.Employees.AnyAsync())
        {
            _logger.LogInformation("Database already seeded.");
            return;
        }

        _logger.LogInformation("Seeding database...");

        // ── Roles ──
        string[] roles = ["Admin", "HR", "Manager", "Employee"];
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
        }

        // ── Shift ──
        var shift = new Shift("Standard", new TimeOnly(9, 0), new TimeOnly(18, 0),
            WorkingDayFlags.Monday | WorkingDayFlags.Tuesday | WorkingDayFlags.Wednesday
            | WorkingDayFlags.Thursday | WorkingDayFlags.Friday);
        shift.ConfigureRules(15, 15, 15, 60, 240, 60);
        _db.Shifts.Add(shift);

        // ── Department ──
        var dept = new Department("Engineering", "ENG", "Software Engineering Department", null);
        _db.Departments.Add(dept);

        // ── Position ──
        var pos = new Position("Software Engineer", "SWE", null, dept.Id);
        _db.Positions.Add(pos);

        // ── Allowances ──
        var housingAllowance = new Allowance("Housing", "HSG", "Monthly housing allowance",
            AllowanceType.Fixed, 2000m, null, AllowanceTaxability.NonTaxable);
        var transportAllowance = new Allowance("Transport", "TRN", "Monthly transport allowance",
            AllowanceType.Fixed, 500m, null, AllowanceTaxability.NonTaxable);
        _db.Allowances.AddRange(housingAllowance, transportAllowance);

        // ── Tax brackets (monthly progressive) ──
        var bracket1 = TaxBracket.Create(0m, 5000m, 0m);
        var bracket2 = TaxBracket.Create(5000m, 15000m, 10m);
        var bracket3 = TaxBracket.Create(15000m, null, 20m);
        var taxSet = new TaxBracketSet("Standard 2024", [bracket1, bracket2, bracket3],
            new DateOnly(2024, 1, 1), null);
        _db.TaxBracketSets.Add(taxSet);

        // ── Social insurance (Egyptian standard rates) ──
        var siConfig = new SocialInsuranceConfig("Standard SI",
            11m, 18.75m, 14000m, new DateOnly(2024, 1, 1), null);
        _db.SocialInsuranceConfigs.Add(siConfig);

        // ── Payroll policy ──
        var policy = new PayrollPolicy("Default 2024", new DateOnly(2024, 1, 1), null,
            26, 8m, 3, 1.5m, "USD");
        _db.PayrollPolicies.Add(policy);

        await _db.SaveChangesAsync();

        // ── Employee ──
        var empCode = EmployeeCode.Create("EMP001");
        var emp = new Employee(empCode, "John", null, "Doe",
            new DateOnly(1990, 5, 15), Gender.Male, "NID123456789",
            dept.Id, pos.Id, new DateOnly(2024, 1, 1));
        emp.AssignShift(shift.Id);
        _db.Employees.Add(emp);
        await _db.SaveChangesAsync();

        // ── Contract + ContractVersion ──
        var baseSalary = Money.Create(10000m, "USD");
        var version = new ContractVersion(1, baseSalary, new DateOnly(2024, 1, 1), null,
            taxSet.Id, siConfig.Id, null);

        version.AddAllowanceAssignment(new AllowanceAssignment(housingAllowance.Id, 2000m, null));
        version.AddAllowanceAssignment(new AllowanceAssignment(transportAllowance.Id, 500m, null));

        var contract = new Contract(emp.Id, ContractType.Permanent,
            new DateOnly(2024, 1, 1), null, version);
        contract.Activate();
        emp.AssignContract(contract);

        await _db.SaveChangesAsync();

        // ── Users ──
        await CreateUser("admin@hrpayroll.com", "Admin@123", "Admin", null);
        await CreateUser("hr@hrpayroll.com", "Hr@123", "HR", null);
        await CreateUser("manager@hrpayroll.com", "Manager@123", "Manager", null);
        await CreateUser("emp001@hrpayroll.com", "Emp@123", "Employee", emp.Id);

        _logger.LogInformation("Seeding complete.");
    }

    private async Task CreateUser(string email, string password, string role, Guid? employeeId)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmployeeId = employeeId,
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
            await _userManager.AddToRoleAsync(user, role);
    }
}
