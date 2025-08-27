namespace CrawlerWebApi.Models;
public class CrawlRequest
{
    public string Url { get; set; } = string.Empty;
    public string SiteId { get; set; } = string.Empty;
    public List<SubjectToResearch>? SubjectsToResearch { get; set; }
}

public class SubjectToResearch
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Examples { get; set; } = new List<string>();
}
