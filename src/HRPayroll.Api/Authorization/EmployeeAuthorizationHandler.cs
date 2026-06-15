using HRPayroll.Application.Common.Security;
using Microsoft.AspNetCore.Authorization;

namespace HRPayroll.Api.Authorization;

public class EmployeeReadRequirement : IAuthorizationRequirement { }

public class EmployeeAuthorizationHandler : AuthorizationHandler<EmployeeReadRequirement, Guid>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EmployeeReadRequirement requirement,
        Guid targetEmployeeId)
    {
        if (context.User.IsInRole("Admin") || context.User.IsInRole("HR"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (context.User.IsInRole("Manager") &&
            context.User.HasClaim(c =>
                c.Type == PayrollClaims.Permission && c.Value == PayrollClaims.Permissions.EmployeeReadAll))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var employeeIdClaim = context.User.FindFirst(PayrollClaims.EmployeeId)?.Value;
        if (employeeIdClaim is not null && Guid.TryParse(employeeIdClaim, out var userEmployeeId))
        {
            if (userEmployeeId == targetEmployeeId &&
                context.User.HasClaim(c =>
                    c.Type == PayrollClaims.Permission && c.Value == PayrollClaims.Permissions.EmployeeReadSelf))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}
