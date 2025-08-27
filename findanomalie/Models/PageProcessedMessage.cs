namespace CrawlerWebApi.Models;


public class PageProcessedMessage
{
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string MetaKeywords { get; set; } = string.Empty;
    public int ContentSize { get; set; }
    public int StatusCode { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public int InternalLinksCount { get; set; }
    public DateTime CrawledAt { get; set; }
    public DateTime ProcessedAt { get; set; }

    public CrawlRequest CrawlRequest { get; set; }
}


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
