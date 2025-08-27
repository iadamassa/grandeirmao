using CheckAnomaliaApi.Models;
using CrawlerWebApi.Models;

namespace CheckAnomaliaApi.Services;

public interface IHasAnomalieService
{
    Task<List<ResultResponse>> AnalyzePageAsync(PageProcessedMessage page);
}
