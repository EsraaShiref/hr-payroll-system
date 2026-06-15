namespace HRPayroll.Domain.Exceptions;

public class InvalidSalaryException(decimal salary)
    : DomainException($"Salary must be greater than zero. Provided: {salary}.");
