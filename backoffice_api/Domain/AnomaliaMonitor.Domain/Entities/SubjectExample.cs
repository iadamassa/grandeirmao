using AnomaliaMonitor.Domain.Common;

namespace AnomaliaMonitor.Domain.Entities;

public class SubjectExample : BaseEntity
{
    public int SubjectToResearchId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Example { get; set; } = string.Empty;

    public virtual SubjectToResearch SubjectToResearch { get; set; } = null!;
}