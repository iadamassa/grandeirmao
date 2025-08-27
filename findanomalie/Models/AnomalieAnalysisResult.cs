namespace CheckAnomaliaApi.Models;

public class AnomalieAnalysisResult
{
    public bool HasAnomalie { get; set; }
    public string Analysis { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public List<string> AnomalieTypes { get; set; } = new();
    public DateTime AnalyzedAt { get; set; }
}