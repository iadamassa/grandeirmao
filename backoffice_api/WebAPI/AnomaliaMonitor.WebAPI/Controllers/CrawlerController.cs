using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AnomaliaMonitor.Application.Services;

namespace AnomaliaMonitor.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CrawlerController : ControllerBase
{
    private readonly ICrawlRequestService _crawlRequestService;
    private readonly ILogger<CrawlerController> _logger;

    public CrawlerController(ICrawlRequestService crawlRequestService, ILogger<CrawlerController> logger)
    {
        _crawlRequestService = crawlRequestService;
        _logger = logger;
    }

    [HttpPost("trigger-all-sites")]
    public async Task<IActionResult> TriggerCrawlForAllSites()
    {
        try
        {
            _logger.LogInformation("Manual trigger requested for crawling all active sites");

            var publishedCount = await _crawlRequestService.PublishCrawlRequestsForAllActiveSitesAsync();

            if (publishedCount == 0)
            {
                return Ok(new 
                { 
                    message = "Nenhum site ativo encontrado para iniciar o crawling",
                    publishedCount = 0,
                    success = true
                });
            }

            return Ok(new 
            { 
                message = $"Crawling iniciado para {publishedCount} site(s) ativo(s)",
                publishedCount,
                success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering crawl for all sites");
            return StatusCode(500, new 
            { 
                message = "Erro interno do servidor ao iniciar o crawling",
                error = ex.Message,
                success = false
            });
        }
    }

    [HttpGet("status")]
    public IActionResult GetCrawlerStatus()
    {
        try
        {
            // This endpoint could be extended to show crawler status/statistics
            return Ok(new 
            { 
                message = "Crawler service is operational",
                timestamp = DateTime.UtcNow,
                success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting crawler status");
            return StatusCode(500, new 
            { 
                message = "Erro ao obter status do crawler",
                error = ex.Message,
                success = false
            });
        }
    }
}