using System.Text.Json.Serialization;

namespace AnomaliaMonitor.Application.DTOs;

public class SubjectExampleDto
{
    public int Id { get; set; }
    public int SubjectToResearchId { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Example { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}