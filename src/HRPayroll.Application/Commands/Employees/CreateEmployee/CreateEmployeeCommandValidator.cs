using FluentValidation;
using HRPayroll.Application.Interfaces;

namespace HRPayroll.Application.Commands.Employees.CreateEmployee;

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator(IEmployeeRepository employeeRepository)
    {
        RuleFor(x => x.EmployeeCode)
            .NotEmpty().WithMessage("Employee code is required.")
            .MaximumLength(20)
            .MustAsync(async (code, ct) => await employeeRepository.IsEmployeeCodeUniqueAsync(code, ct))
                .WithMessage("Employee code already exists.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Date of birth must be in the past.");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required.")
            .Must(g => g is "Male" or "Female").WithMessage("Gender must be 'Male' or 'Female'.");

        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("National ID is required.")
            .Length(5, 50)
            .MustAsync(async (nationalId, ct) => await employeeRepository.IsNationalIdUniqueAsync(nationalId, ct))
                .WithMessage("National ID already exists.");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department is required.");

        RuleFor(x => x.PositionId)
            .NotEmpty().WithMessage("Position is required.");

        RuleFor(x => x.HireDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Hire date cannot be in the future.");
    }
}
