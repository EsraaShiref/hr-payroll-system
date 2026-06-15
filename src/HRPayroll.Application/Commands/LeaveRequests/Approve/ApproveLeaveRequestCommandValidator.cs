using FluentValidation;

namespace HRPayroll.Application.Commands.LeaveRequests.Approve;

public class ApproveLeaveRequestCommandValidator : AbstractValidator<ApproveLeaveRequestCommand>
{
    public ApproveLeaveRequestCommandValidator()
    {
        RuleFor(x => x.LeaveRequestId).NotEmpty();
        RuleFor(x => x.ApprovedBy).NotEmpty();
    }
}
