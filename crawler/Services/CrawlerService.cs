using System.Collections.Concurrent;
using System.IO.Compression;
using System.Net;
using System.Text;

using HtmlAgilityPack;
using Microsoft.Extensions.Http;
using CrawlerWebApi.Models;
using MassTransit;

namespace CrawlerWebApi.Services;

public class CrawlerService : ICrawlerService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Random _random;
    private readonly IPublishEndpoint _publishEndpoint;

    // Define o número máximo de requisições que podem ser executadas em paralelo.
    private const int MaxConcurrency = 5; // Reduzido para ser mais "humano"

    // Delays para simular comportamento humano
    private const int MinDelayMs = 500;   // Mínimo 500ms entre requisições
    private const int MaxDelayMs = 3000;  // Máximo 3s entre requisições

    public CrawlerService(IHttpClientFactory httpClientFactory, IPublishEndpoint publishEndpoint)
    {
        _httpClientFactory = httpClientFactory;
        _publishEndpoint = publishEndpoint;
        _random = new Random();
    }

    public async Task<IEnumerable<string>> FindInternalLinksAsync(string baseUrl, CancellationToken cancellationToken, CrawlRequest request)
    {
        var result = await ExtractCompleteDataAsync(baseUrl, cancellationToken, request);
        return result.Pages.Where(p => !p.HasError).Select(p => p.Url);
    }

    public async Task<CrawlResult> ExtractCompleteDataAsync(string baseUrl, CancellationToken cancellationToken, CrawlRequest request)
    {
        var result = new CrawlResult
        {
            BaseUrl = baseUrl,
            StartedAt = DateTime.UtcNow
        };

        var baseUri = new Uri(baseUrl);
        var allPages = new ConcurrentBag<PageData>();
        var urlsToCrawl = new ConcurrentQueue<string>();
        var crawledUrls = new ConcurrentDictionary<string, bool>();
        var processingUrls = new ConcurrentDictionary<string, bool>();

        urlsToCrawl.Enqueue(baseUrl);
        crawledUrls.TryAdd(baseUrl, true);

        var semaphore = new SemaphoreSlim(MaxConcurrency);
        var tasks = new List<Task>();

        Console.WriteLine($"🚀 Iniciando extração completa para: {baseUrl}");
        Console.WriteLine($"📊 Status inicial: {crawledUrls.Count} URLs marcadas para processamento");

        while (urlsToCrawl.Any() || tasks.Count > 0)
        {
            while (urlsToCrawl.TryDequeue(out var currentUrl))
            {
                if (processingUrls.ContainsKey(currentUrl))
                {
                    Console.WriteLine($"⚠️  {currentUrl} já está sendo processada, pulando...");
                    continue;
                }

                processingUrls.TryAdd(currentUrl, true);
                await semaphore.WaitAsync(cancellationToken);

                var task = Task.Run(async () =>
                {
                    var pageData = new PageData { Url = currentUrl };

                    try
                    {
                        var delay = _random.Next(MinDelayMs, MaxDelayMs);
                        await Task.Delay(delay, cancellationToken);

                        Console.WriteLine($"🔍 Extraindo dados de: {currentUrl} (Total processado: {crawledUrls.Count})");

                        var client = _httpClientFactory.CreateClient("CrawlerClient");
                        AddDynamicHeaders(client);

                        var response = await client.GetAsync(currentUrl, cancellationToken);
                        pageData.StatusCode = (int)response.StatusCode;
                        pageData.ContentType = response.Content.Headers.ContentType?.MediaType ?? "";

                        if (!response.IsSuccessStatusCode)
                        {
                            pageData.HasError = true;
                            pageData.ErrorMessage = $"HTTP {response.StatusCode}: {response.ReasonPhrase}";
                            allPages.Add(pageData);
                            Console.WriteLine($"❌ Erro HTTP {response.StatusCode} em {currentUrl}");
                            return;
                        }

                        var contentType = response.Content.Headers.ContentType?.MediaType;
                        if (contentType != null && !contentType.Contains("text/html"))
                        {
                            Console.WriteLine($"⚠️  Pulando {currentUrl} - não é HTML ({contentType})");
                            pageData.HasError = true;
                            pageData.ErrorMessage = $"Conteúdo não é HTML: {contentType}";
                            allPages.Add(pageData);
                            return;
                        }

                        string html = await ReadContentSafelyAsync(response, cancellationToken);

                        if (string.IsNullOrEmpty(html))
                        {
                            Console.WriteLine($"⚠️  Conteúdo vazio para {currentUrl}");
                            pageData.HasError = true;
                            pageData.ErrorMessage = "Conteúdo HTML vazio";
                            allPages.Add(pageData);
                            return;
                        }

                        // ✅ EXTRAIR DADOS COMPLETOS DA PÁGINA
                        pageData.HtmlContent = html;
                        pageData.ContentSize = html.Length;
                        ExtractPageMetadata(pageData, html);

                        var doc = new HtmlDocument();
                        doc.LoadHtml(html);

                        var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]");
                        if (linkNodes != null)
                        {
                            var foundLinks = 0;
                            var duplicateLinks = 0;

                            foreach (var node in linkNodes)
                            {
                                var href = node.GetAttributeValue("href", string.Empty);
                                if (string.IsNullOrEmpty(href)) continue;

                                var absoluteUri = ToAbsoluteUri(currentUrl, href);
                                if (absoluteUri == null) continue;

                                var absoluteUrlString = absoluteUri.GetLeftPart(UriPartial.Path);

                                if (IsInternalLink(baseUri, absoluteUri))
                                {
                                    pageData.InternalLinks.Add(absoluteUrlString);

                                    if (crawledUrls.TryAdd(absoluteUrlString, true))
                                    {
                                        urlsToCrawl.Enqueue(absoluteUrlString);
                                        foundLinks++;
                                    }
                                    else
                                    {
                                        duplicateLinks++;
                                    }
                                }
                            }

                            // Remove duplicatas da lista de links da página
                            pageData.InternalLinks = pageData.InternalLinks.Distinct().ToList();

                            Console.WriteLine($"✅ {currentUrl} - Extraído HTML ({pageData.ContentSize:N0} chars), " +
                                            $"Título: '{pageData.Title}', {foundLinks} novos links, {duplicateLinks} duplicados");
                        }
                        else
                        {
                            Console.WriteLine($"ℹ️  {currentUrl} - HTML extraído ({pageData.ContentSize:N0} chars), " +
                                            $"Título: '{pageData.Title}', nenhum link encontrado");
                        }

                        // ✅ PROCESSAR PÁGINA COM SUCESSO - Chamar método específico
                        await ProcessSuccessfulPageAsync(pageData, cancellationToken, request);

                        allPages.Add(pageData);

                        if (crawledUrls.Count % 10 == 0)
                        {
                            Console.WriteLine($"📊 Progresso: {crawledUrls.Count} páginas processadas, {urlsToCrawl.Count} na fila, " +
                                            $"{allPages.Count(p => !p.HasError)} com sucesso");
                        }
                    }
                    catch (HttpRequestException ex) when (ex.Data.Contains("StatusCode"))
                    {
                        var statusCode = ex.Data["StatusCode"];
                        Console.WriteLine($"❌ Erro HTTP {statusCode} em {currentUrl}: {ex.Message}");
                        pageData.HasError = true;
                        pageData.ErrorMessage = $"HTTP {statusCode}: {ex.Message}";
                        allPages.Add(pageData);

                        if (statusCode?.ToString() == "429" || statusCode?.ToString() == "503")
                        {
                            var extraDelay = _random.Next(5000, 10000);
                            await Task.Delay(extraDelay, cancellationToken);
                        }
                    }
                    catch (HttpRequestException ex) when (ex.Message.Contains("404"))
                    {
                        Console.WriteLine($"🔍 URL não encontrada (404): {currentUrl}");
                        pageData.HasError = true;
                        pageData.ErrorMessage = "Página não encontrada (404)";
                        allPages.Add(pageData);
                    }
                    catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                    {
                        Console.WriteLine($"⏱️  Timeout ao processar {currentUrl}");
                        pageData.HasError = true;
                        pageData.ErrorMessage = "Timeout na requisição";
                        allPages.Add(pageData);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"💥 Falha ao processar {currentUrl}: {ex.Message}");
                        pageData.HasError = true;
                        pageData.ErrorMessage = ex.Message;
                        allPages.Add(pageData);

                        var errorDelay = _random.Next(2000, 5000);
                        await Task.Delay(errorDelay, cancellationToken);
                    }
                    finally
                    {
                        processingUrls.TryRemove(currentUrl, out _);
                        semaphore.Release();
                    }
                }, cancellationToken);

                tasks.Add(task);
            }

            if (tasks.Any())
            {
                var completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);
                try
                {
                    await completedTask;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️  Erro em tarefa assíncrona: {ex.Message}");
                }
            }
        }

        result.Pages = allPages.OrderBy(p => p.Url).ToList();
        result.CompletedAt = DateTime.UtcNow;

        Console.WriteLine($"🎉 Extração completa concluída!");
        Console.WriteLine($"📊 Estatísticas finais:");
        foreach (var stat in result.Statistics)
        {
            Console.WriteLine($"   • {stat.Key}: {stat.Value}");
        }

        return result;
    }

    /// <summary>
    /// Processa uma página que foi extraída com sucesso e envia para a fila do MassTransit
    /// </summary>
    private async Task ProcessSuccessfulPageAsync(PageData pageData, CancellationToken cancellationToken, CrawlRequest request)
    {
        try
        {
            // Verificar se a página foi processada com sucesso
            if (pageData.HasError)
            {
                Console.WriteLine($"⚠️  Página {pageData.Url} tem erro, não será enviada para a fila");
                return;
            }

            // Criar mensagem para enviar ao MassTransit
            var pageProcessedMessage = new PageProcessedMessage
            {
                Url = pageData.Url,
                Title = pageData.Title,
                HtmlContent = pageData.HtmlContent,
                MetaDescription = pageData.MetaDescription,
                MetaKeywords = pageData.MetaKeywords,
                ContentSize = pageData.ContentSize,
                StatusCode = pageData.StatusCode,
                ContentType = pageData.ContentType,
                InternalLinksCount = pageData.InternalLinks.Count,
                CrawledAt = pageData.CrawledAt,
                ProcessedAt = DateTime.UtcNow,
                CrawlRequest = request
            };

            // Publicar mensagem na fila do MassTransit
            await _publishEndpoint.Publish(pageProcessedMessage, cancellationToken);

            Console.WriteLine($"📤 Página {pageData.Url} enviada para fila de processamento - " +
                            $"Título: '{TruncateString(pageData.Title, 30)}', " +
                            $"Tamanho: {pageData.ContentSize:N0} chars");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao enviar página {pageData.Url} para fila: {ex.Message}");
            // Não relançar exceção para não interromper o crawling
        }
    }

    /// <summary>
    /// Extrai metadados da página (título, descrição, keywords, etc.)
    /// </summary>
    private void ExtractPageMetadata(PageData pageData, string html)
    {
        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Extrair título
            var titleNode = doc.DocumentNode.SelectSingleNode("//title");
            pageData.Title = titleNode?.InnerText?.Trim() ?? "";

            // Extrair meta description
            var descriptionNode = doc.DocumentNode.SelectSingleNode("//meta[@name='description']") ??
                                 doc.DocumentNode.SelectSingleNode("//meta[@property='og:description']");
            pageData.MetaDescription = descriptionNode?.GetAttributeValue("content", "")?.Trim() ?? "";

            // Extrair meta keywords
            var keywordsNode = doc.DocumentNode.SelectSingleNode("//meta[@name='keywords']");
            pageData.MetaKeywords = keywordsNode?.GetAttributeValue("content", "")?.Trim() ?? "";

            // Limpar textos extraídos
            pageData.Title = System.Net.WebUtility.HtmlDecode(pageData.Title);
            pageData.MetaDescription = System.Net.WebUtility.HtmlDecode(pageData.MetaDescription);
            pageData.MetaKeywords = System.Net.WebUtility.HtmlDecode(pageData.MetaKeywords);

            Console.WriteLine($"📄 Metadados extraídos - Título: '{TruncateString(pageData.Title, 50)}' | " +
                            $"Descrição: {pageData.MetaDescription.Length} chars");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Erro ao extrair metadados: {ex.Message}");
        }
    }

    /// <summary>
    /// Lê o conteúdo da resposta HTTP de forma segura, lidando com diferentes tipos de compressão
    /// </summary>
    private async Task<string> ReadContentSafelyAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            // Verificar o Content-Type para garantir que é texto/HTML
            var contentType = response.Content.Headers.ContentType?.MediaType?.ToLowerInvariant();
            if (contentType != null && !contentType.Contains("text") && !contentType.Contains("html"))
            {
                Console.WriteLine($"⚠️  Conteúdo não é texto. Content-Type: {contentType}");
                return string.Empty;
            }

            // Verificar encoding de compressão
            var contentEncoding = response.Content.Headers.ContentEncoding;

            // Log do tipo de compressão detectado
            if (contentEncoding.Any())
            {
                Console.WriteLine($"🔄 Detectada compressão: {string.Join(", ", contentEncoding)}");
            }

            // Tratar compressão Brotli
            if (contentEncoding.Contains("br"))
            {
                Console.WriteLine("🔄 Descomprimindo conteúdo Brotli...");
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var brotliStream = new BrotliStream(stream, CompressionMode.Decompress);
                using var reader = new StreamReader(brotliStream, Encoding.UTF8);
                return await reader.ReadToEndAsync();
            }
            // Tratar compressão Gzip
            else if (contentEncoding.Contains("gzip"))
            {
                Console.WriteLine("🔄 Descomprimindo conteúdo Gzip...");
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
                using var reader = new StreamReader(gzipStream, Encoding.UTF8);
                return await reader.ReadToEndAsync();
            }
            // Tratar compressão Deflate
            else if (contentEncoding.Contains("deflate"))
            {
                Console.WriteLine("🔄 Descomprimindo conteúdo Deflate...");
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var deflateStream = new DeflateStream(stream, CompressionMode.Decompress);
                using var reader = new StreamReader(deflateStream, Encoding.UTF8);
                return await reader.ReadToEndAsync();
            }
            else
            {
                // Sem compressão, mas verificar charset
                var charset = response.Content.Headers.ContentType?.CharSet ?? "UTF-8";
                Console.WriteLine($"📄 Lendo conteúdo sem compressão (charset: {charset})...");

                try
                {
                    var encoding = Encoding.GetEncoding(charset);
                    var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                    return encoding.GetString(bytes);
                }
                catch (ArgumentException)
                {
                    // Se o charset não for reconhecido, usar UTF-8 como fallback
                    Console.WriteLine($"⚠️  Charset '{charset}' não reconhecido, usando UTF-8 como fallback");
                    var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                    return Encoding.UTF8.GetString(bytes);
                }
            }
        }
        catch (InvalidDataException ex)
        {
            Console.WriteLine($"❌ Erro de dados inválidos ao descomprimir: {ex.Message}");

            // Tentar ler como string simples em caso de erro de descompressão
            try
            {
                Console.WriteLine("🔄 Tentando ler como conteúdo não comprimido...");
                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (Exception fallbackEx)
            {
                Console.WriteLine($"❌ Falha no fallback: {fallbackEx.Message}");
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao ler conteúdo da resposta: {ex.Message}");

            // Último recurso: tentar ReadAsStringAsync
            try
            {
                Console.WriteLine("🔄 Tentando fallback para ReadAsStringAsync...");
                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (Exception fallbackEx)
            {
                Console.WriteLine($"❌ Falha no fallback final: {fallbackEx.Message}");
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// Adiciona headers dinâmicos para cada requisição, simulando melhor um navegador real
    /// </summary>
    private void AddDynamicHeaders(HttpClient client)
    {
        // Remove headers existentes que podem ser adicionados dinamicamente
        client.DefaultRequestHeaders.Remove("Referer");
        client.DefaultRequestHeaders.Remove("X-Requested-With");

        // Simula referer ocasionalmente (como se viesse de outro site)
        if (_random.Next(1, 10) <= 2) // 20% de chance
        {
            var referers = new[]
            {
                "https://www.google.com/",
                "https://www.bing.com/",
                "https://duckduckgo.com/"
            };
            client.DefaultRequestHeaders.Add("Referer", referers[_random.Next(referers.Length)]);
        }

        // Ocasionalmente adiciona X-Requested-With (como se fosse uma requisição AJAX)
        if (_random.Next(1, 20) == 1) // 5% de chance
        {
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        }
    }

    /// <summary>
    /// Converte uma URL relativa ou absoluta em um objeto Uri absoluto.
    /// </summary>
    private Uri? ToAbsoluteUri(string baseUrl, string relativeOrAbsoluteUrl)
    {
        try
        {
            var baseUri = new Uri(baseUrl);

            // Trata URLs que começam com "//"
            if (relativeOrAbsoluteUrl.StartsWith("//"))
            {
                relativeOrAbsoluteUrl = baseUri.Scheme + ":" + relativeOrAbsoluteUrl;
            }

            // Ignora links para outros protocolos (mailto, tel, etc.)
            if (relativeOrAbsoluteUrl.StartsWith("mailto:") ||
                relativeOrAbsoluteUrl.StartsWith("tel:") ||
                relativeOrAbsoluteUrl.StartsWith("javascript:") ||
                relativeOrAbsoluteUrl.StartsWith("data:"))
            {
                return null;
            }

            return new Uri(baseUri, relativeOrAbsoluteUrl);
        }
        catch
        {
            // Ignora URLs malformadas.
            return null;
        }
    }

    /// <summary>
    /// Verifica se um link pertence ao mesmo domínio da URL base.
    /// </summary>
    private bool IsInternalLink(Uri baseUri, Uri linkUri)
    {
        return baseUri.Host.Equals(linkUri.Host, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Trunca uma string para exibição em logs
    /// </summary>
    private string TruncateString(string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            return input;

        return input.Substring(0, maxLength) + "...";
    }
}
