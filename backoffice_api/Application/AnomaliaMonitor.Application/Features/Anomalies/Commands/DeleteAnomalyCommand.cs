using MediatR;

namespace AnomaliaMonitor.Application.Features.Anomalies.Commands;

public class DeleteAnomalyCommand : IRequest<bool>
{
    public int Id { get; set; }
}