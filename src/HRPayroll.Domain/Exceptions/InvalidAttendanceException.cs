namespace HRPayroll.Domain.Exceptions;

public class InvalidAttendanceException(string message) : DomainException(message);
