using FluentValidation;

namespace HRPayroll.Application.Commands.LeaveRequests.Submit;

public class SubmitLeaveRequestCommandValidator : AbstractValidator<SubmitLeaveRequestCommand>
{
    public SubmitLeaveRequestCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.LeaveType).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty();
        RuleFor(x => x).Must(x => x.EndDate >= x.StartDate)
            .WithMessage("End date must be on or after start date.");
    }
}
