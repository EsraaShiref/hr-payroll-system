using ErrorOr;
using HRPayroll.Application.DTOs.Dashboard;
using MediatR;

namespace HRPayroll.Application.Queries.Dashboard.GetHeadcountTrends;

public sealed record GetHeadcountTrendsQuery(
    int Months = 6) : IRequest<ErrorOr<HeadcountTrendDto>>;
