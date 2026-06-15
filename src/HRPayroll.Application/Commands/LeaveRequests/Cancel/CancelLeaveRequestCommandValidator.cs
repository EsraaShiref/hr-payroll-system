using FluentValidation;

namespace HRPayroll.Application.Commands.LeaveRequests.Cancel;

public class CancelLeaveRequestCommandValidator : AbstractValidator<CancelLeaveRequestCommand>
{
    public CancelLeaveRequestCommandValidator()
    {
        RuleFor(x => x.LeaveRequestId).NotEmpty();
    }
}
