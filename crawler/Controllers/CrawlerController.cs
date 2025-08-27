
using Microsoft.AspNetCore.Mvc;
using MassTransit;
using CrawlerWebApi.Models;

namespace CrawlerWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CrawlerController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CrawlerController> _logger;

    public CrawlerController(IPublishEndpoint publishEndpoint, ILogger<CrawlerController> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint para enviar uma solicitação de crawling para a fila RabbitMQ
    /// (Opcional - para testes)
    /// </summary>
    [HttpPost("crawl")]
    public async Task<IActionResult> RequestCrawl([FromBody] CrawlRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
        {
            return BadRequest("URL é obrigatória");
        }

        try
        {
            await _publishEndpoint.Publish(request);

            _logger.LogInformation("Solicitação de crawling enviada para a fila - URL: {Url} ",
                request.Url);

            return Accepted(new {
                Message = "Solicitação de crawling enviada para processamento"

            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar solicitação de crawling para URL: {Url}", request.Url);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Endpoint para verificar o status da API
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Message = "CrawlerWebApi está funcionando"
        });
    }
}


