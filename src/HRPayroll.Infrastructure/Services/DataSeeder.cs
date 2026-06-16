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

        Employee? emp = null;
        using var txn = await _db.Database.BeginTransactionAsync();

        try
        {
            // ── Level 1: Shift, Department, Position, Allowances, Tax, SI, Policy ──
            var shift = new Shift("Standard", new TimeOnly(9, 0), new TimeOnly(18, 0),
                WorkingDayFlags.Monday | WorkingDayFlags.Tuesday | WorkingDayFlags.Wednesday
                | WorkingDayFlags.Thursday | WorkingDayFlags.Friday);
            shift.ConfigureRules(15, 15, 15, 60, 240, 60);
            _db.Shifts.Add(shift);

            var dept = new Department("Engineering", "ENG", "Software Engineering Department", null);
            _db.Departments.Add(dept);

            var pos = new Position("Software Engineer", "SWE", null, dept.Id);
            _db.Positions.Add(pos);

            var housingAllowance = new Allowance("Housing", "HSG", "Monthly housing allowance",
                AllowanceType.Fixed, 2000m, null, AllowanceTaxability.NonTaxable);
            var transportAllowance = new Allowance("Transport", "TRN", "Monthly transport allowance",
                AllowanceType.Fixed, 500m, null, AllowanceTaxability.NonTaxable);
            _db.Allowances.AddRange(housingAllowance, transportAllowance);

            var bracket1 = TaxBracket.Create(0m, 5000m, 0m);
            var bracket2 = TaxBracket.Create(5000m, 15000m, 10m);
            var bracket3 = TaxBracket.Create(15000m, null, 20m);
            var taxSet = new TaxBracketSet("Standard 2024", [bracket1, bracket2, bracket3],
                new DateOnly(2024, 1, 1), null);
            _db.TaxBracketSets.Add(taxSet);

            var siConfig = new SocialInsuranceConfig("Standard SI",
                11m, 18.75m, 14000m, new DateOnly(2024, 1, 1), null);
            _db.SocialInsuranceConfigs.Add(siConfig);

            var policy = new PayrollPolicy("Default 2024", new DateOnly(2024, 1, 1), null,
                26, 8m, 3, 1.5m, "USD");
            _db.PayrollPolicies.Add(policy);

            await _db.SaveChangesAsync();

            // ── Level 2: Employee ──
            var empCode = EmployeeCode.Create("EMP001");
            emp = new Employee(empCode, "John", null, "Doe",
                new DateOnly(1990, 5, 15), Gender.Male, "NID123456789",
                dept.Id, pos.Id, new DateOnly(2024, 1, 1));
            emp.AssignShift(shift.Id);
            _db.Employees.Add(emp);
            await _db.SaveChangesAsync();

            // ── Level 3: Contract + ContractVersion + AllowanceAssignments (single batch) ──
            var baseSalary = Money.Create(10000m, "USD");
            var version = new ContractVersion(1, baseSalary, new DateOnly(2024, 1, 1), null,
                taxSet.Id, siConfig.Id, null);

            var housingAssignment = new AllowanceAssignment(housingAllowance.Id, 2000m, null);
            var transportAssignment = new AllowanceAssignment(transportAllowance.Id, 500m, null);
            housingAssignment.SetContractVersionId(version.Id);
            transportAssignment.SetContractVersionId(version.Id);
            version.AddAllowanceAssignment(housingAssignment);
            version.AddAllowanceAssignment(transportAssignment);

            var contract = new Contract(emp.Id, ContractType.Permanent,
                new DateOnly(2024, 1, 1), null, version);
            contract.Activate();

            _db.Contracts.Add(contract);
            await _db.SaveChangesAsync();

            await txn.CommitAsync();
            _logger.LogInformation("Seeded domain entities.");
        }
        catch (Exception ex)
        {
            await txn.RollbackAsync();
            _logger.LogError(ex, "Failed to seed domain entities.");
            throw;
        }

        // ── Users (outside transaction — Identity uses its own store) ──
        await CreateUser("admin@hrpayroll.com", "Admin@123", "Admin", null);
        await CreateUser("hr@hrpayroll.com", "Hr@123", "HR", null);
        await CreateUser("manager@hrpayroll.com", "Manager@123", "Manager", null);
        if (emp is not null)
            await CreateUser("emp001@hrpayroll.com", "Emp@123", "Employee", emp.Id);

        _logger.LogInformation("Seeding complete.");
    }

    private async Task CreateUser(string email, string password, string role, Guid? employeeId)
    {
        var user = new ApplicationUser { UserName = email, Email = email, EmployeeId = employeeId };
        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
            await _userManager.AddToRoleAsync(user, role);
    }
}
