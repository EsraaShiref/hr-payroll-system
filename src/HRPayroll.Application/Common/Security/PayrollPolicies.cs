namespace HRPayroll.Application.Common.Security;

public static class PayrollPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string HrOnly = "HrOnly";
    public const string HrOrManager = "HrOrManager";
    public const string EmployeeReadAccess = "EmployeeReadAccess";
    public const string PayrollManage = "PayrollManage";
}
