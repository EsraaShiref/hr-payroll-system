using ErrorOr;
using HRPayroll.Application.DTOs;
using MediatR;

namespace HRPayroll.Application.Queries.Holidays.GetHolidaysList;

public sealed record GetHolidaysListQuery(int? Year = null) : IRequest<ErrorOr<List<HolidayDto>>>;
