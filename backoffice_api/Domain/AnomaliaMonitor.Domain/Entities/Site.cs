using AnomaliaMonitor.Domain.Common;

namespace AnomaliaMonitor.Domain.Entities;

public class Site : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastVerification { get; set; }

    public virtual ICollection<SiteCategory> Categories { get; set; } = new List<SiteCategory>();
    public virtual ICollection<SiteLink> Links { get; set; } = new List<SiteLink>();
}