using AnomaliaMonitor.Domain.Entities;

namespace AnomaliaMonitor.Application.Services;

public interface ICrawlRequestService
{
    Task PublishCrawlRequestAsync(Site site);
    Task<int> PublishCrawlRequestsForAllActiveSitesAsync();
}