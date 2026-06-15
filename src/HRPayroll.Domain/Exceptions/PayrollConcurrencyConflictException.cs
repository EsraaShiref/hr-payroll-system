namespace HRPayroll.Domain.Exceptions;

public class PayrollConcurrencyConflictException(string message) : DomainException(message);
