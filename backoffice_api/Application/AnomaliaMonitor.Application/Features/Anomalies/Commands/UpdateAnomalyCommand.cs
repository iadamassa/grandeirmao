using MediatR;
using AnomaliaMonitor.Application.DTOs;

namespace AnomaliaMonitor.Application.Features.Anomalies.Commands;

public class UpdateAnomalyCommand : IRequest<AnomalyDto>
{
    public int Id { get; set; }
    public int SiteLinkId { get; set; }
    public int SubjectToResearchId { get; set; }
    public DateTimeOffset EstimatedDateTime { get; set; }
    public string IdentifiedSubject { get; set; } = string.Empty;
    public string ExampleOrReason { get; set; } = string.Empty;
}