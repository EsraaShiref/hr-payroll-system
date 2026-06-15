namespace HRPayroll.Domain.Exceptions;

public class InvalidEmployeeStatusException(string message)
    : DomainException(message);
