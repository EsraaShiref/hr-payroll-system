using HRPayroll.Domain.Enums;
using HRPayroll.Domain.Events;
using HRPayroll.Domain.Exceptions;
using HRPayroll.Domain.ValueObjects;
using MediatR;

namespace HRPayroll.Domain.Entities;

public class Employee : BaseEntity
{
    public EmployeeCode EmployeeCode { get; private set; } = null!;
    public string FirstName { get; private set; } = string.Empty;
    public string? MiddleName { get; private set; }
    public string LastName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public string NationalId { get; private set; } = string.Empty;
    public string? PersonalEmail { get; private set; }
    public string? PendingNewEmail { get; private set; }
    public bool IsEmailChangePending { get; private set; }
    public string? PhoneNumber { get; private set; }
    public Address? Address { get; private set; }
    public string? EmergencyContactName { get; private set; }
    public string? EmergencyContactPhone { get; private set; }

    public Guid DepartmentId { get; private set; }
    public Department Department { get; private set; } = null!;
    public Guid PositionId { get; private set; }
    public Position Position { get; private set; } = null!;
    public Guid? ShiftId { get; private set; }
    public Shift? Shift { get; private set; }

    public EmploymentStatus EmploymentStatus { get; private set; }
    public DateOnly HireDate { get; private set; }
    public DateOnly? TerminationDate { get; private set; }
    public string? TerminationReason { get; private set; }

    public ICollection<Contract> Contracts { get; private set; } = new List<Contract>();

    private readonly List<INotification> _domainEvents = new();
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    private Employee() { }

    public Employee(
        EmployeeCode employeeCode,
        string firstName,
        string? middleName,
        string lastName,
        DateOnly dateOfBirth,
        Gender gender,
        string nationalId,
        Guid departmentId,
        Guid positionId,
        DateOnly hireDate)
    {
        SetEmployeeCode(employeeCode);
        SetFirstName(firstName);
        MiddleName = middleName;
        SetLastName(lastName);
        SetDateOfBirth(dateOfBirth);
        Gender = gender;
        SetNationalId(nationalId);
        DepartmentId = departmentId;
        PositionId = positionId;
        HireDate = hireDate;
        EmploymentStatus = EmploymentStatus.Active;

        _domainEvents.Add(new EmployeeCreatedEvent(Id, employeeCode.Value, departmentId, positionId, DateTime.UtcNow));
    }

    public string FullName => $"{FirstName}{(MiddleName != null ? $" {MiddleName}" : "")} {LastName}";

    public Contract? ActiveContract =>
        Contracts.FirstOrDefault(c => c.Status == ContractStatus.Active);

    public void AssignContract(Contract contract)
    {
        if (contract == null)
            throw new ArgumentNullException(nameof(contract));

        if (contract.EmployeeId != Id)
            throw new ArgumentException("Contract must reference this employee.", nameof(contract));

        if (ActiveContract != null)
            throw new DuplicateActiveContractException(Id);

        if (contract.SignedDate < HireDate)
            throw new InvalidContractDateRangeException("Contract SignedDate cannot be before HireDate.");

        Contracts.Add(contract);
    }

    public void Terminate(DateOnly terminationDate, string reason)
    {
        if (EmploymentStatus != EmploymentStatus.Active)
            throw new InvalidEmployeeStatusException($"Employee {Id} is {EmploymentStatus} and cannot be terminated.");

        EmploymentStatus = EmploymentStatus.Terminated;
        TerminationDate = terminationDate;
        TerminationReason = reason;

        var activeContract = ActiveContract;
        if (activeContract != null)
        {
            activeContract.Terminate(terminationDate, $"Employee terminated: {reason}");
        }

        _domainEvents.Add(new EmployeeTerminatedEvent(Id, terminationDate, reason, DateTime.UtcNow));
    }

    public void TransferDepartment(Guid newDepartmentId, Guid newPositionId)
    {
        DepartmentId = newDepartmentId;
        PositionId = newPositionId;
    }

    public void AssignShift(Guid shiftId) => ShiftId = shiftId;

    public void ClearShiftOverride() => ShiftId = null;

    public void UpdatePersonalInfo(
        string? phoneNumber,
        Address? address,
        string? emergencyContactName,
        string? emergencyContactPhone)
    {
        PhoneNumber = phoneNumber;
        Address = address;
        EmergencyContactName = emergencyContactName;
        EmergencyContactPhone = emergencyContactPhone;
    }

    public void RequestEmailChange(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new ArgumentException("Email is required.", nameof(newEmail));
        if (!newEmail.Contains('@'))
            throw new ArgumentException("Invalid email format.", nameof(newEmail));

        PendingNewEmail = newEmail;
        IsEmailChangePending = true;
    }

    public void ApproveEmailChange()
    {
        if (!IsEmailChangePending)
            throw new InvalidOperationException("No pending email change to approve.");
        PersonalEmail = PendingNewEmail;
        PendingNewEmail = null;
        IsEmailChangePending = false;
    }

    public void RejectEmailChange()
    {
        PendingNewEmail = null;
        IsEmailChangePending = false;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void SetEmployeeCode(EmployeeCode code)
    {
        EmployeeCode = code ?? throw new ArgumentNullException(nameof(code));
    }

    private void SetFirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));
        if (firstName.Length > 100)
            throw new ArgumentException("First name cannot exceed 100 characters.", nameof(firstName));
        FirstName = firstName.Trim();
    }

    private void SetLastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.", nameof(lastName));
        if (lastName.Length > 100)
            throw new ArgumentException("Last name cannot exceed 100 characters.", nameof(lastName));
        LastName = lastName.Trim();
    }

    private void SetDateOfBirth(DateOnly dateOfBirth)
    {
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Date of birth cannot be in the future.", nameof(dateOfBirth));
        DateOfBirth = dateOfBirth;
    }

    private void SetNationalId(string nationalId)
    {
        if (string.IsNullOrWhiteSpace(nationalId))
            throw new ArgumentException("National ID is required.", nameof(nationalId));
        if (nationalId.Length is < 5 or > 50)
            throw new ArgumentException("National ID must be between 5 and 50 characters.", nameof(nationalId));
        NationalId = nationalId.Trim();
    }
}
