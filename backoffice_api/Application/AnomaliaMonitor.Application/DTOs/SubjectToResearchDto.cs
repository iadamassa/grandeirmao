namespace AnomaliaMonitor.Application.DTOs;

public class SubjectToResearchDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public List<SubjectExampleDto> Examples { get; set; } = new();
    public List<SiteCategoryDto> Categories { get; set; } = new();
}