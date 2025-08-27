

namespace CrawlerWebApi.Models;

/// <summary>
/// Mensagem enviada para a fila quando uma página é processada com sucesso
/// </summary>
public class PageProcessedMessage
{
    /// <summary>
    /// URL da página processada
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Título da página
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Conteúdo HTML completo da página
    /// </summary>
    public string HtmlContent { get; set; } = string.Empty;

    /// <summary>
    /// Meta descrição da página
    /// </summary>
    public string MetaDescription { get; set; } = string.Empty;

    /// <summary>
    /// Meta keywords da página
    /// </summary>
    public string MetaKeywords { get; set; } = string.Empty;

    /// <summary>
    /// Tamanho do conteúdo em caracteres
    /// </summary>
    public int ContentSize { get; set; }

    /// <summary>
    /// Status HTTP da resposta
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Content-Type da resposta
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Quantidade de links internos encontrados
    /// </summary>
    public int InternalLinksCount { get; set; }

    /// <summary>
    /// Data/hora em que a página foi extraída
    /// </summary>
    public DateTime CrawledAt { get; set; }

    /// <summary>
    /// Data/hora em que a mensagem foi processada
    /// </summary>
    public DateTime ProcessedAt { get; set; }

    public CrawlRequest CrawlRequest { get; set; }
}
