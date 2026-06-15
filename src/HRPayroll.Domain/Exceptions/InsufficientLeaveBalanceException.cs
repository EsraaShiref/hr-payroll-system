namespace HRPayroll.Domain.Exceptions;

public class InsufficientLeaveBalanceException(Guid employeeId, Enums.LeaveType leaveType, int requested, int available)
    : DomainException($"Employee {employeeId} has insufficient {leaveType} leave balance. Requested: {requested}, Available: {available}.")
{
    public Guid EmployeeId => employeeId;
    public Enums.LeaveType LeaveType => leaveType;
    public int Requested => requested;
    public int Available => available;
}
