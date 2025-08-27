using CrawlerWebApi.Controllers;

using MassTransit;
using CrawlerWebApi.Models;
using CrawlerWebApi.Services;

namespace CrawlerWebApi.Consumers;

public class CrawlRequestConsumer : IConsumer<CrawlRequest>
{
    private readonly ICrawlerService _crawlerService;
    private readonly ILogger<CrawlRequestConsumer> _logger;
    private static readonly SemaphoreSlim _semaphore = new(1, 1); // Permite apenas 1 processamento por vez

    public CrawlRequestConsumer(ICrawlerService crawlerService, ILogger<CrawlRequestConsumer> logger)
    {
        _crawlerService = crawlerService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CrawlRequest> context)
    {
        var message = context.Message;

        _logger.LogInformation("Recebida mensagem de crawling para URL: {Url}",
            message.Url);

        // Aguarda até que seja possível processar (garante que apenas uma mensagem seja processada por vez)
        await _semaphore.WaitAsync(context.CancellationToken);

        try
        {
            _logger.LogInformation("Iniciando crawling para URL: {Url}", message.Url);

            var result = await _crawlerService.ExtractCompleteDataAsync(message.Url, context.CancellationToken, message);

            _logger.LogInformation("Crawling concluído para URL: {Url}. " +
                "Páginas processadas: {TotalPages}, Sucessos: {SuccessfulPages}, Falhas: {FailedPages}, " +
                "Links únicos: {TotalUniqueLinks}, Duração: {Duration}",
                message.Url,
                result.Pages.Count,
                result.SuccessfulPages,
                result.FailedPages,
                result.TotalUniqueLinks,
                result.Duration);

            await SaveResultsAsync(result);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operação de crawling cancelada para URL: {Url}", message.Url);
            throw; // Rethrow para que o MassTransit saiba que o processamento foi cancelado
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar crawling para URL: {Url}", message.Url);
            throw; // Re-throw para trigger retry policy se configurado
        }
        finally
        {
            _semaphore.Release();
            _logger.LogInformation("Liberado semáforo de processamento");
        }
    }

    private async Task SaveResultsAsync(CrawlResult result)
    {
        // Implementar lógica para salvar os resultados,
        // Por exemplo: salvar em banco de dados, enviar para outra fila, etc.

        _logger.LogInformation("Salvando resultados do crawling - Base URL: {BaseUrl}, Total de páginas: {TotalPages}",
            result.BaseUrl, result.Pages.Count);

        // Exemplo: salvar em arquivo (remover em produção)
        var fileName = $"crawl_result_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{new Uri(result.BaseUrl).Host}.json";
        var json = System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(fileName, json);
        _logger.LogInformation("Resultados salvos em arquivo: {FileName}", fileName);
    }
}
