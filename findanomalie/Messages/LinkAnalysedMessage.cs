using CrawlerWebApi.Models;

namespace CrawlerWebApi.Models;

public class LinkAnalysedMessage
{
    public string UrlLink { get; set; }
    public bool HasAnomalie { get; set; }
    public string Analysis { get; set; } = string.Empty;
    public CrawlRequest CrawlRequest { get; set; }
    public SubjectToResearch SubjectsResearched { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    public Boolean SuccessAnalyze { get; set; } = true;
}
