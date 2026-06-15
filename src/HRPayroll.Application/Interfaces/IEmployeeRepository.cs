using HRPayroll.Domain.Entities;

namespace HRPayroll.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Employee?> GetByEmployeeCodeAsync(string employeeCode, CancellationToken ct = default);
    Task<Employee?> GetWithContractsAsync(Guid id, CancellationToken ct = default);
    Task<Employee?> GetWithDepartmentAndShiftAsync(Guid id, CancellationToken ct = default);
    Task<List<Employee>> GetAllActiveAsync(CancellationToken ct = default);
    Task<bool> IsEmployeeCodeUniqueAsync(string employeeCode, CancellationToken ct = default);
    Task<bool> IsNationalIdUniqueAsync(string nationalId, CancellationToken ct = default);
    void Add(Employee employee);
    void Update(Employee employee);
}
