namespace CrawlerWebApi.Models;

public class CrawlRequest
{
    public string Url { get; set; } = string.Empty;
    public string SiteId { get; set; } = string.Empty;
    public List<SubjectToResearch>? SubjectsToResearch { get; set; }
}
