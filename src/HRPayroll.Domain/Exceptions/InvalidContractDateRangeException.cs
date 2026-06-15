namespace HRPayroll.Domain.Exceptions;

public class InvalidContractDateRangeException(string message)
    : DomainException(message);
