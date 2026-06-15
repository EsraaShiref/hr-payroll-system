namespace HRPayroll.Domain.Exceptions;

public class DuplicatePunchException(Guid employeeId, DateTime timestamp)
    : DomainException($"Duplicate punch for employee {employeeId} at {timestamp:O}. A {nameof(Enums.PunchType)} punch already exists within the configured deduplication window.")
{
    public Guid EmployeeId => employeeId;
    public DateTime Timestamp => timestamp;
}
