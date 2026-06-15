namespace HRPayroll.Domain.Exceptions;

public class InvalidLeaveBalanceOperationException(string message) : DomainException(message);
