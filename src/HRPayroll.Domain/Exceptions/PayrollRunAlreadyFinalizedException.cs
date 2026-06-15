namespace HRPayroll.Domain.Exceptions;

public class PayrollRunAlreadyFinalizedException(string message) : DomainException(message);
