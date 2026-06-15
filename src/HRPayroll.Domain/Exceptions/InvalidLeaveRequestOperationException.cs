namespace HRPayroll.Domain.Exceptions;

public class InvalidLeaveRequestOperationException(string message) : DomainException(message);
