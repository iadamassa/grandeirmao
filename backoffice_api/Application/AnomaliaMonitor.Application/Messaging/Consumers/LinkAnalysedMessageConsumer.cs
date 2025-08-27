using MassTransit;
using Microsoft.Extensions.Logging;
using AnomaliaMonitor.Domain.Interfaces;
using AnomaliaMonitor.Domain.Entities;
using CrawlerWebApi.Models;

namespace AnomaliaMonitor.Application.Messaging.Consumers;

public class LinkAnalysedMessageConsumer : IConsumer<LinkAnalysedMessage>
{
    private readonly ILogger<LinkAnalysedMessageConsumer> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public LinkAnalysedMessageConsumer(ILogger<LinkAnalysedMessageConsumer> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<LinkAnalysedMessage> context)
    {
        var message = context.Message;

        _logger.LogInformation("Received LinkAnalysedMessage for URL: {Url}, HasAnomalie: {HasAnomalie}, SuccessAnalyze: {SuccessAnalyze}",
            message.CrawlRequest.Url, message.HasAnomalie, message.SuccessAnalyze);

        try
        {
            if (!message.SuccessAnalyze)
            {
                _logger.LogError("Analysis failed for URL: {Url}", message.CrawlRequest.Url);
                return;
            }

            // Parse SiteId from string to int
            if (!int.TryParse(message.CrawlRequest.SiteId, out int siteId))
            {
                _logger.LogError("Invalid SiteId format: {SiteId} for URL: {Url}", message.CrawlRequest.SiteId, message.CrawlRequest.Url);
                return;
            }

            // Check if SiteLink exists with the URL and SiteId
            var existingSiteLink = await FindExistingSiteLinkAsync(message.UrlLink, siteId);

            if (existingSiteLink == null)
            {
                // Cria novo SiteLink (já está tracked como Added)
                existingSiteLink = await CreateSiteLinkAsync(message.UrlLink, siteId, message.HasAnomalie);
                _logger.LogInformation("Created new SiteLink for URL: {Url}, SiteId: {SiteId}", message.UrlLink, siteId);
            }
            else
            {
                // Só faz Update se já existe no banco
                existingSiteLink.IsAnalyzed = true;
                existingSiteLink.HasAnomaly = message.HasAnomalie;
                _unitOfWork.SiteLinks.Update(existingSiteLink);
            }

            await _unitOfWork.SaveChangesAsync(); // Garante que o Id foi gerado


            // Create anomaly if detected
            if (message.HasAnomalie && message.SubjectsResearched != null)
            {
                await CreateAnomalyAsync(existingSiteLink.Id, message.SubjectsResearched.Id, message.Analysis, message.AnalyzedAt);
                _logger.LogWarning("Anomaly created for URL: {Url}. Analysis: {Analysis}", message.UrlLink, message.Analysis);
            }
            else
            {
                _logger.LogInformation("No anomaly detected for URL: {Url}", message.UrlLink);
            }

            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing LinkAnalysedMessage for URL: {Url}", message.CrawlRequest.Url);
            throw;
        }
    }

    private async Task<SiteLink?> FindExistingSiteLinkAsync(string url, int siteId)
    {
        var siteLinks = await _unitOfWork.SiteLinks.FindAsync(sl => sl.Url == url && sl.SiteId == siteId);
        return siteLinks.FirstOrDefault();
    }

    private async Task<SiteLink> CreateSiteLinkAsync(string url, int siteId, bool hasAnomalie)
    {
        var siteLink = new SiteLink
        {
            Url = url,
            SiteId = siteId,
            Name = ExtractNameFromUrl(url),
            Description = $"Auto-created from analysis of {url}",
            IsActive = true,
            IsAnalyzed = true,
            HasAnomaly = hasAnomalie
        };

        return await _unitOfWork.SiteLinks.AddAsync(siteLink);
    }

    private async Task CreateAnomalyAsync(int siteLinkId, int subjectToResearchId, string analysis, DateTimeOffset analyzedAt)
    {
        var anomaly = new Anomaly
        {
            SiteLinkId = siteLinkId,
            SubjectToResearchId = subjectToResearchId,
            EstimatedDateTime = analyzedAt,
            IdentifiedSubject = "Anomaly detected via automated analysis",
            ExampleOrReason = analysis
        };

        await _unitOfWork.Anomalies.AddAsync(anomaly);
    }

    private static string ExtractNameFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath.Trim('/');
            return string.IsNullOrEmpty(path) ? uri.Host : path.Split('/').Last();
        }
        catch
        {
            return url;
        }
    }
}
