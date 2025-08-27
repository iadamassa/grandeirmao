namespace AnomaliaMonitor.Application.DTOs;

public class SiteLinkDto
{
    public int Id { get; set; }
    public int SiteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsAnalyzed { get; set; }
    public bool HasAnomaly { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string SiteName { get; set; } = string.Empty;
}