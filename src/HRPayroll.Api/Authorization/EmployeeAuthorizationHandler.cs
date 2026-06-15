using HRPayroll.Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace HRPayroll.Api.Authorization;

public class EmployeeAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Guid>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Guid targetEmployeeId)
    {
        // Admin and HR can access any employee
        if (context.User.IsInRole("Admin") || context.User.IsInRole("HR"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Resource-based check: employee accessing their own record
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

        // Manager with employee:read:all can access any employee
        if (context.User.IsInRole("Manager") &&
            context.User.HasClaim(c =>
                c.Type == PayrollClaims.Permission && c.Value == PayrollClaims.Permissions.EmployeeReadAll))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
