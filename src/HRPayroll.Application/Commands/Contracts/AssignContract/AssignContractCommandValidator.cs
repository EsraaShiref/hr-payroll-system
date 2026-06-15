using FluentValidation;

namespace HRPayroll.Application.Commands.Contracts.AssignContract;

public class AssignContractCommandValidator : AbstractValidator<AssignContractCommand>
{
    public AssignContractCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty();

        RuleFor(x => x.ContractType)
            .NotEmpty()
            .Must(t => t is "Permanent" or "FixedTerm" or "Probation" or "Internship")
            .WithMessage("Contract type must be Permanent, FixedTerm, Probation, or Internship.");

        RuleFor(x => x.BaseSalaryAmount)
            .GreaterThan(0).WithMessage("Base salary must be greater than zero.");

        RuleFor(x => x.BaseSalaryCurrency)
            .NotEmpty().Length(3);

        RuleFor(x => x.SignedDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));

        RuleFor(x => x)
            .Must(x => x.ContractType != "FixedTerm" || x.ExpiryDate.HasValue)
            .WithMessage("Expiry date is required for fixed-term contracts.")
            .Must(x => !x.ExpiryDate.HasValue || x.ExpiryDate > x.SignedDate)
            .WithMessage("Expiry date must be after signed date.");
    }
}
