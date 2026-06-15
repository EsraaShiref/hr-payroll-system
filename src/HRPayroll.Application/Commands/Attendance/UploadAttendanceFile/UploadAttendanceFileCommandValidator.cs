using ErrorOr;
using FluentValidation;

namespace HRPayroll.Application.Commands.Attendance.UploadAttendanceFile;

public class UploadAttendanceFileCommandValidator : AbstractValidator<UploadAttendanceFileCommand>
{
    public UploadAttendanceFileCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.FileContent).NotEmpty();
        RuleFor(x => x).Must(x =>
        {
            var ext = Path.GetExtension(x.FileName)?.ToLowerInvariant();
            return ext is ".xlsx" or ".csv";
        }).WithMessage("File must be .xlsx or .csv format.");
    }
}
