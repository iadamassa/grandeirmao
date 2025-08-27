using MediatR;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.Dashboard.Queries;

public class GetDashboardDataQuery : IRequest<DashboardDto>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}