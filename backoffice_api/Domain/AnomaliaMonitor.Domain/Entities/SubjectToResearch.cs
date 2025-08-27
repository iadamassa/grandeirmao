using AnomaliaMonitor.Domain.Common;

namespace AnomaliaMonitor.Domain.Entities;

public class SubjectToResearch : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public virtual ICollection<SubjectExample> Examples { get; set; } = new List<SubjectExample>();
    public virtual ICollection<SiteCategory> Categories { get; set; } = new List<SiteCategory>();
    public virtual ICollection<Anomaly> Anomalies { get; set; } = new List<Anomaly>();
}