using HRPayroll.Domain.Enums;
using HRPayroll.Domain.Exceptions;
using MediatR;

namespace HRPayroll.Domain.Entities;

public class LeaveBalance : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public Employee Employee { get; private set; } = null!;
    public LeaveType LeaveType { get; private set; }
    public int Year { get; private set; }
    public decimal TotalDays { get; private set; }
    public decimal UsedDays { get; private set; }

    public decimal RemainingDays => TotalDays - UsedDays;

    private readonly List<INotification> _domainEvents = new();
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    private LeaveBalance() { }

    public LeaveBalance(Guid employeeId, LeaveType leaveType, int year, decimal totalDays)
    {
        EmployeeId = employeeId;
        LeaveType = leaveType;
        Year = year;
        TotalDays = totalDays;
        UsedDays = 0;
    }

    public void Deduct(decimal days)
    {
        if (days <= 0)
            throw new ArgumentException("Days to deduct must be positive.", nameof(days));
        if (RemainingDays < days)
            throw new InsufficientLeaveBalanceException(EmployeeId, LeaveType, (int)days, (int)RemainingDays);

        UsedDays += days;
    }

    public void Credit(decimal days)
    {
        if (days <= 0)
            throw new ArgumentException("Days to credit must be positive.", nameof(days));
        if (UsedDays < days)
            throw new InvalidLeaveBalanceOperationException("Cannot credit more days than used.");

        UsedDays -= days;
    }

    public void SetTotalDays(decimal totalDays)
    {
        if (totalDays < 0)
            throw new ArgumentException("Total days cannot be negative.", nameof(totalDays));
        if (totalDays < UsedDays)
            throw new InvalidLeaveBalanceOperationException("Total days cannot be less than used days.");

        TotalDays = totalDays;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
