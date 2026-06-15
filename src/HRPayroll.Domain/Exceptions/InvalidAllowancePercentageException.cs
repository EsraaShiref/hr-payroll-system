namespace HRPayroll.Domain.Exceptions;

public class InvalidAllowancePercentageException(decimal percentage)
    : DomainException($"Allowance percentage must be between 0 and 100. Provided: {percentage}.");
