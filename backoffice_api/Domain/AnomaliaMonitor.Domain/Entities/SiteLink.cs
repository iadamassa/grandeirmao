using AnomaliaMonitor.Domain.Common;

namespace AnomaliaMonitor.Domain.Entities;

public class SiteLink : BaseEntity
{
    public int SiteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsAnalyzed { get; set; } = false;
    public bool HasAnomaly { get; set; } = false;

    public virtual Site Site { get; set; } = null!;
    public virtual ICollection<Anomaly> Anomalies { get; set; } = new List<Anomaly>();
}