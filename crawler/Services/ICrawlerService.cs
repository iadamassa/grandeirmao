
using CrawlerWebApi.Models;

namespace CrawlerWebApi.Services;

public interface ICrawlerService
{
    Task<IEnumerable<string>> FindInternalLinksAsync(string baseUrl, CancellationToken cancellationToken, CrawlRequest request);
    Task<CrawlResult> ExtractCompleteDataAsync(string baseUrl, CancellationToken cancellationToken, CrawlRequest request);
}
