namespace AnomaliaMonitor.Application.DTOs;

public class AnomalyDto
{
    public int Id { get; set; }
    public int SiteLinkId { get; set; }
    public int SubjectToResearchId { get; set; }
    public DateTimeOffset EstimatedDateTime { get; set; }
    public string IdentifiedSubject { get; set; } = string.Empty;
    public string ExampleOrReason { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string SiteLinkName { get; set; } = string.Empty;
    public string SiteLinkUrl { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
}