namespace HRPayroll.Domain.Exceptions;

public class DuplicateActiveContractException(Guid employeeId)
    : DomainException($"Employee {employeeId} already has an active contract. Terminate it before assigning a new one.");
