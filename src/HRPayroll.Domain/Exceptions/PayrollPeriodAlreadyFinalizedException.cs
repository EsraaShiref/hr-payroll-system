namespace HRPayroll.Domain.Exceptions;

public class PayrollPeriodAlreadyFinalizedException(string message) : DomainException(message);
