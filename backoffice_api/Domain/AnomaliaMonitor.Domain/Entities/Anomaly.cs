using AnomaliaMonitor.Domain.Common;

namespace AnomaliaMonitor.Domain.Entities;

public class Anomaly : BaseEntity
{
    public int SiteLinkId { get; set; }
    public int SubjectToResearchId { get; set; }
    public DateTimeOffset EstimatedDateTime { get; set; }
    public string IdentifiedSubject { get; set; } = string.Empty;
    public string ExampleOrReason { get; set; } = string.Empty;

    public virtual SiteLink SiteLink { get; set; } = null!;
    public virtual SubjectToResearch SubjectToResearch { get; set; } = null!;
}