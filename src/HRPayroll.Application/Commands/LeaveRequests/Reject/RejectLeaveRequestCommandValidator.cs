using FluentValidation;

namespace HRPayroll.Application.Commands.LeaveRequests.Reject;

public class RejectLeaveRequestCommandValidator : AbstractValidator<RejectLeaveRequestCommand>
{
    public RejectLeaveRequestCommandValidator()
    {
        RuleFor(x => x.LeaveRequestId).NotEmpty();
        RuleFor(x => x.RejectedBy).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
