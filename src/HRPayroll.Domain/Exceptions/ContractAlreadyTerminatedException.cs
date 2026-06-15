namespace HRPayroll.Domain.Exceptions;

public class ContractAlreadyTerminatedException(Guid contractId)
    : DomainException($"Contract {contractId} is already terminated.");
