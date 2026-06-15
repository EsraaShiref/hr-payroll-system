namespace HRPayroll.Application.Common.Security;

public static class PayrollClaims
{
    public const string EmployeeId = "employeeId";
    public const string Permission = "permission";

    public static class Permissions
    {
        public const string EmployeeReadSelf = "employee:read:self";
        public const string EmployeeReadAll = "employee:read:all";
        public const string EmployeeCreate = "employee:create";
        public const string EmployeeTerminate = "employee:terminate";
        public const string ContractCreate = "contract:create";
        public const string ContractActivate = "contract:activate";
        public const string ContractTerminate = "contract:terminate";
        public const string PayrollRun = "payroll:run";
        public const string PayrollApprove = "payroll:approve";
        public const string DepartmentManage = "department:manage";
        public const string Administer = "administer";
    }
}
