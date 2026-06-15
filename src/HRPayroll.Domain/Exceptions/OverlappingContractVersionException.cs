namespace HRPayroll.Domain.Exceptions;

public class OverlappingContractVersionException(DateOnly proposedFrom, DateOnly? proposedTo, DateOnly conflictingFrom, DateOnly? conflictingTo)
    : DomainException(
        $"Contract version {proposedFrom:yyyy-MM-dd}–{proposedTo:yyyy-MM-dd} overlaps " +
        $"with existing version {conflictingFrom:yyyy-MM-dd}–{conflictingTo:yyyy-MM-dd}.");
