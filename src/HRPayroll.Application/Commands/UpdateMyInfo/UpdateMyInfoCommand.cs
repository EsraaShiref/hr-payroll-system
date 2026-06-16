using ErrorOr;
using MediatR;

namespace HRPayroll.Application.Commands.UpdateMyInfo;

public sealed record UpdateMyInfoCommand(
    string? PhoneNumber,
    string? Street,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    string? EmergencyContactName,
    string? EmergencyContactPhone) : IRequest<ErrorOr<Success>>;
