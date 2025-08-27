namespace CrawlerWebApi.Models;

/// <summary>
/// Resultado completo da operação de crawling
/// </summary>
public class CrawlResult
{
    /// <summary>
    /// URL base que foi utilizada para o crawling
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Lista de todas as páginas extraídas com seus dados
    /// </summary>
    public List<PageData> Pages { get; set; } = new();

    /// <summary>
    /// Data/hora de início do crawling
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Data/hora de fim do crawling
    /// </summary>
    public DateTime CompletedAt { get; set; }

    /// <summary>
    /// Duração total do crawling
    /// </summary>
    public TimeSpan Duration => CompletedAt - StartedAt;

    /// <summary>
    /// Total de páginas processadas com sucesso
    /// </summary>
    public int SuccessfulPages => Pages.Count(p => !p.HasError);

    /// <summary>
    /// Total de páginas com erro
    /// </summary>
    public int FailedPages => Pages.Count(p => p.HasError);

    /// <summary>
    /// Total de links únicos encontrados
    /// </summary>
    public int TotalUniqueLinks => Pages.SelectMany(p => p.InternalLinks).Distinct().Count();

    /// <summary>
    /// Estatísticas do crawling
    /// </summary>
    public Dictionary<string, object> Statistics => new()
    {
        ["TotalPages"] = Pages.Count,
        ["SuccessfulPages"] = SuccessfulPages,
        ["FailedPages"] = FailedPages,
        ["TotalUniqueLinks"] = TotalUniqueLinks,
        ["AveragePageSize"] = Pages.Where(p => !p.HasError).Average(p => p.ContentSize),
        ["Duration"] = Duration.ToString(@"hh\:mm\:ss"),
        ["PagesPerMinute"] = Pages.Count / Math.Max(Duration.TotalMinutes, 1)
    };
}