using System.Text.Json.Serialization;

namespace CrawlerWebApi.Models;

/// <summary>
/// Representa os dados extraídos de uma página web
/// </summary>
public class PageData
{
    /// <summary>
    /// URL da página
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Conteúdo HTML completo da página
    /// </summary>
    public string HtmlContent { get; set; } = string.Empty;

    /// <summary>
    /// Título da página (extraído do HTML)
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Descrição meta da página
    /// </summary>
    public string MetaDescription { get; set; } = string.Empty;

    /// <summary>
    /// Keywords meta da página
    /// </summary>
    public string MetaKeywords { get; set; } = string.Empty;

    /// <summary>
    /// Data/hora da extração
    /// </summary>
    public DateTime CrawledAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tamanho do conteúdo HTML em bytes
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
    /// Lista de links internos encontrados nesta página
    /// </summary>
    public List<string> InternalLinks { get; set; } = new();

    /// <summary>
    /// Indica se houve erro na extração
    /// </summary>
    public bool HasError { get; set; }

    /// <summary>
    /// Mensagem de erro (se houver)
    /// </summary>
    public string? ErrorMessage { get; set; }
}