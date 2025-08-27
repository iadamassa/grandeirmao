using Microsoft.AspNetCore.Mvc;
using MassTransit;
using AnomaliaMonitor.Application.Messaging;
using CrawlerWebApi.Models;

namespace AnomaliaMonitor.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<TestController> _logger;

    public TestController(IPublishEndpoint publishEndpoint, ILogger<TestController> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    [HttpPost("publish-test-message")]
    public async Task<IActionResult> PublishTestMessage()
    {
        try
        {
            var testMessage = new LinkAnalysedMessage
            {
                HasAnomalie = true,
                Analysis = "Test analysis - anomaly detected in test URL",
                CrawlRequest = new CrawlRequest
                {
                    Url = "https://test.example.com",
                    SiteId = "1",
                    SubjectsToResearch = new List<SubjectToResearch>
                    {
                        new SubjectToResearch
                        {
                            Id = 1,
                            Name = "Test Subject",
                            Description = "Test subject for anomaly detection",
                            Examples = new List<string> { "test example 1", "test example 2" }
                        }
                    }
                },
                SubjectsResearched = new SubjectToResearch
                {
                    Id = 1,
                    Name = "Test Subject",
                    Description = "Test subject for anomaly detection",
                    Examples = new List<string> { "test example 1", "test example 2" }
                },
                AnalyzedAt = DateTime.Now,
                SuccessAnalyze = true
            };

            await _publishEndpoint.Publish(testMessage);

            _logger.LogInformation("Test message published successfully");
            return Ok(new { message = "Test message published to linkanalised queue" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing test message");
            return StatusCode(500, new { error = "Failed to publish test message", details = ex.Message });
        }
    }

    [HttpPost("publish-crawl-request")]
    public async Task<IActionResult> PublishCrawlRequest()
    {
        try
        {
            var testCrawlRequest = new CrawlRequest
            {
                Url = "https://test-site.example.com",
                SiteId = "1",
                SubjectsToResearch = new List<SubjectToResearch>
                {
                    new SubjectToResearch
                    {
                        Id = 1,
                        Name = "Test Subject for Crawling",
                        Description = "Test subject for crawl request",
                        Examples = new List<string> { "test crawl example 1", "test crawl example 2" }
                    }
                }
            };

            await _publishEndpoint.Publish(testCrawlRequest, context =>
            {
                context.SetRoutingKey("crawl-requests");
            });

            _logger.LogInformation("Test crawl request published successfully");
            return Ok(new { message = "Test crawl request published to crawl-requests queue" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing test crawl request");
            return StatusCode(500, new { error = "Failed to publish test crawl request", details = ex.Message });
        }
    }

    [HttpPost("simulate-anomaly-flags")]
    public async Task<IActionResult> SimulateAnomalyFlags()
    {
        try
        {
            // This endpoint is for testing purposes - simulates setting HasAnomaly flags
            // In production, this would be done by the LinkAnalysedMessageConsumer

            _logger.LogInformation("Simulating anomaly flags for testing dashboard functionality");
            return Ok(new { message = "This endpoint would simulate setting HasAnomaly flags on SiteLinks for testing purposes" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error simulating anomaly flags");
            return StatusCode(500, new { error = "Failed to simulate anomaly flags", details = ex.Message });
        }
    }
}
