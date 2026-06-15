using HRPayroll.Domain.Entities;

namespace HRPayroll.Application.Interfaces;

public interface IContractRepository
{
    Task<Contract?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Contract?> GetWithVersionsAsync(Guid id, CancellationToken ct = default);
    Task<Contract?> GetActiveForEmployeeAsync(Guid employeeId, CancellationToken ct = default);
    Task<ContractVersion?> GetEffectiveVersionAsync(Guid employeeId, DateOnly effectiveDate, CancellationToken ct = default);
    void Add(Contract contract);
}
