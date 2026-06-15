using FluentValidation;

namespace HRPayroll.Application.Commands.Contracts.AddContractVersion;

public class AddContractVersionCommandValidator : AbstractValidator<AddContractVersionCommand>
{
    public AddContractVersionCommandValidator()
    {
        RuleFor(x => x.ContractId)
            .NotEmpty();

        RuleFor(x => x.NewBaseSalaryAmount)
            .GreaterThan(0);

        RuleFor(x => x.NewBaseSalaryCurrency)
            .NotEmpty().Length(3);

        RuleFor(x => x.EffectiveFrom)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));

        RuleForEach(x => x.AllowanceAssignments)
            .SetValidator(new AllowanceAssignmentInputValidator());
    }
}

public class AllowanceAssignmentInputValidator : AbstractValidator<AllowanceAssignmentInput>
{
    public AllowanceAssignmentInputValidator()
    {
        RuleFor(x => x.AllowanceId)
            .NotEmpty();

        RuleFor(x => x.OverrideAmount)
            .GreaterThan(0).When(x => x.OverrideAmount.HasValue);

        RuleFor(x => x.OverridePercentage)
            .InclusiveBetween(0, 100).When(x => x.OverridePercentage.HasValue);

        RuleFor(x => x)
            .Must(x => !(x.OverrideAmount.HasValue && x.OverridePercentage.HasValue))
            .WithMessage("Provide either an override amount or percentage, not both.");
    }
}
