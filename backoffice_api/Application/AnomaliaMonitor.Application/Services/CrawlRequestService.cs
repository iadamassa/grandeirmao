using MassTransit;
using Microsoft.Extensions.Logging;
using AnomaliaMonitor.Domain.Entities;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Application.Messaging;
using CrawlerWebApi.Models;
using SubjectToResearch = CrawlerWebApi.Models.SubjectToResearch;

namespace AnomaliaMonitor.Application.Services;

public class CrawlRequestService : ICrawlRequestService
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CrawlRequestService> _logger;

    public CrawlRequestService(
        IPublishEndpoint publishEndpoint,
        IUnitOfWork unitOfWork,
        ILogger<CrawlRequestService> logger)
    {
        _publishEndpoint = publishEndpoint;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task PublishCrawlRequestAsync(Site site)
    {
        try
        {
            // Get active subjects from the site's categories
            var activeSubjects = await GetActiveSubjectsFromSiteAsync(site);

            var crawlRequest = new CrawlRequest
            {
                Url = site.Url,
                SiteId = site.Id.ToString(),
                SubjectsToResearch = activeSubjects
            };

           await _publishEndpoint.Publish(crawlRequest);

            _logger.LogInformation("Published crawl request for Site: {SiteId}, URL: {Url}, Subjects: {SubjectCount}",
                site.Id, site.Url, activeSubjects?.Count ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing crawl request for Site: {SiteId}, URL: {Url}", site.Id, site.Url);
            throw;
        }
    }

    public async Task<int> PublishCrawlRequestsForAllActiveSitesAsync()
    {
        try
        {
            _logger.LogInformation("Starting bulk crawl request publishing for all active sites");

            // Get all active sites with their categories
            var activeSites = await _unitOfWork.Sites.GetAllWithIncludesAsync("Categories");
            var activeSitesFiltered = activeSites.Where(s => s.IsActive).ToList();

            if (!activeSitesFiltered.Any())
            {
                _logger.LogWarning("No active sites found for crawl request publishing");
                return 0;
            }

            int publishedCount = 0;

            // Process sites sequentially to avoid DbContext concurrency issues
            foreach (var site in activeSitesFiltered)
            {
                try
                {
                    await PublishCrawlRequestAsync(site);
                    publishedCount++;
                    _logger.LogDebug("Successfully published crawl request for Site: {SiteId}, Name: {SiteName}",
                        site.Id, site.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish crawl request for Site: {SiteId}, Name: {SiteName}",
                        site.Id, site.Name);
                    // Continue processing other sites even if one fails
                }
            }

            _logger.LogInformation("Bulk crawl request publishing completed. Successfully published: {PublishedCount}/{TotalSites}",
                publishedCount, activeSitesFiltered.Count);

            return publishedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk crawl request publishing");
            throw;
        }
    }

    private async Task<List<SubjectToResearch>> GetActiveSubjectsFromSiteAsync(Site site)
    {
        if (!site.Categories.Any())
        {
            return null;
        }

        var activeSubjects = new List<SubjectToResearch>();

        foreach (var category in site.Categories)
        {
            // Get category with its active subjects and examples
            var categoryWithSubjects = await _unitOfWork.SiteCategories
                .GetByIdWithIncludesAsync(category.Id, "Subjects", "Subjects.Examples");

            if (categoryWithSubjects?.Subjects != null)
            {
                var activeCategorySubjects = categoryWithSubjects.Subjects
                    .Where(s => s.IsActive)
                    .Select(s => new SubjectToResearch
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        Examples = s.Examples?.Select(e => e.Example).ToList() ?? new List<string>()
                    })
                    .ToList();

                activeSubjects.AddRange(activeCategorySubjects);
            }
        }

        // Remove duplicates (in case a subject belongs to multiple categories)
        var distinctSubjects = activeSubjects
            .GroupBy(s => s.Id)
            .Select(g => g.First())
            .ToList();

        return distinctSubjects.Any() ? distinctSubjects : null;
    }
}
