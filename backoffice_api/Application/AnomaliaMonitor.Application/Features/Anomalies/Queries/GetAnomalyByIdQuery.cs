using MediatR;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.Anomalies.Queries;

public class GetAnomalyByIdQuery : IRequest<AnomalyDto?>
{
    public int Id { get; set; }
}