using AnomaliaMonitor.Domain.Common;

namespace AnomaliaMonitor.Domain.Entities;

public class SiteCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#000000";
    public bool IsActive { get; set; } = true;

    public virtual ICollection<SubjectToResearch> Subjects { get; set; } = new List<SubjectToResearch>();
    public virtual ICollection<Site> Sites { get; set; } = new List<Site>();
}