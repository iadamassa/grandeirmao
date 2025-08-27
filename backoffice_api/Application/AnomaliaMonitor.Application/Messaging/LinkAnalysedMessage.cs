namespace CrawlerWebApi.Models;
public class LinkAnalysedMessage
{
    public string UrlLink { get; set; }

    public bool HasAnomalie { get; set; }
    public string Analysis { get; set; } = string.Empty;
    public CrawlRequest CrawlRequest { get; set; } = new();
    public SubjectToResearch SubjectsResearched { get; set; } = new();
    public DateTimeOffset AnalyzedAt { get; set; } = DateTime.UtcNow;
    public bool SuccessAnalyze { get; set; } = true;
}
