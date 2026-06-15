namespace HRPayroll.Domain.Exceptions;

public class InvalidPayrollRunStateException(string message) : DomainException(message);
