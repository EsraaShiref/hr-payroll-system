using System.Text;
using System.Text.Json.Serialization;
using HRPayroll.Api.Authorization;
using HRPayroll.Api.Middleware;
using HRPayroll.Application;
using HRPayroll.Application.Common.Security;
using HRPayroll.Infrastructure;
using HRPayroll.Infrastructure.Services;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Layer registrations
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Hangfire server
builder.Services.AddHangfireServer();

// JWT Authentication
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>()!;
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = signingKey,
        ClockSkew = TimeSpan.Zero,
    };

    options.MapInboundClaims = false;
});

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PayrollPolicies.AdminOnly, policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy(PayrollPolicies.HrOnly, policy =>
        policy.RequireRole("Admin", "HR"));

    options.AddPolicy(PayrollPolicies.HrOrManager, policy =>
        policy.RequireRole("Admin", "HR", "Manager"));

    options.AddPolicy(PayrollPolicies.EmployeeReadAccess, policy =>
        policy.Requirements.Add(new EmployeeReadRequirement()));

    options.AddPolicy(PayrollPolicies.PayrollManage, policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") ||
            (context.User.IsInRole("HR") && context.User.HasClaim(c =>
                c.Type == PayrollClaims.Permission && c.Value == PayrollClaims.Permissions.PayrollRun)) ||
            (context.User.IsInRole("Manager") && context.User.HasClaim(c =>
                c.Type == PayrollClaims.Permission && c.Value == PayrollClaims.Permissions.PayrollApprove))));
});

// Register custom authorization handler
builder.Services.AddScoped<IAuthorizationHandler, EmployeeAuthorizationHandler>();

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "HR Payroll API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!);

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Hangfire dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.MapControllers();
app.MapHealthChecks("/health");

try
{
    Log.Information("Starting HR Payroll API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
