
using CheckAnomaliaApi.Models;
using CheckAnomaliaApi.Services;
using CrawlerWebApi.Models;
using MassTransit;

namespace CheckAnomaliaApi.Consumers;

public class PageProcessedConsumer(
  IPublishEndpoint _publishEndpoint,
  IHasAnomalieService _hasAnomalieService,
  ILogger<PageProcessedConsumer> _logger
) : IConsumer<PageProcessedMessage>
{

    public async Task Consume(ConsumeContext<PageProcessedMessage> context)
    {
        var message = context.Message;

        _logger.LogInformation("📄 Recebida página processada: {Url}", message.Url);
        _logger.LogInformation("🏷️  Título: {Title}", message.Title);
        _logger.LogInformation("📊 Tamanho: {Size} chars, Links: {Links}",
            message.ContentSize, message.InternalLinksCount);

        try
        {
            var analysisResult = await _hasAnomalieService.AnalyzePageAsync(message);

            foreach (var item in analysisResult)
            {
                var l =  new LinkAnalysedMessage
                {
                    // Mapeie as propriedades conforme necessário
                    CrawlRequest = item.CrawlRequest,
                    Analysis = item.Analysis,
                    SubjectsResearched = item.SubjectsResearched,
                    AnalyzedAt = item.AnalyzedAt,
                    HasAnomalie = item.HasAnomalie,
                    SuccessAnalyze = item.SuccessAnalyze,
                    UrlLink = message.Url,
                };

                await _publishEndpoint.Publish(l);

            }

            _logger.LogInformation("🔍 Análise concluída para {Url}", message.Url);


        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao analisar página {Url}", message.Url);
            throw;
        }
    }
}
