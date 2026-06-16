using ErrorOr;
using HRPayroll.Application.DTOs.Dashboard;
using MediatR;

namespace HRPayroll.Application.Queries.Dashboard.GetUpcomingContractRenewals;

public sealed record GetUpcomingContractRenewalsQuery(
    int DaysAhead = 30) : IRequest<ErrorOr<List<UpcomingContractRenewalDto>>>;
