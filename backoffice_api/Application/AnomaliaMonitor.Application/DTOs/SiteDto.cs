namespace AnomaliaMonitor.Application.DTOs;

public class SiteDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset? LastVerification { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public List<SiteLinkDto> Links { get; set; } = new();
    public List<SiteCategoryDto> Categories { get; set; } = new();
}