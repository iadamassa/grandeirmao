using CheckAnomaliaApi.Models;
using System.Text;
using System.Text.Json;
using CrawlerWebApi.Models;
using HtmlAgilityPack;


namespace CheckAnomaliaApi.Services;

public class HasAnomalieService : IHasAnomalieService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HasAnomalieService> _logger;
    private readonly string _ollamaUrl;
    private readonly string _model;
    private readonly string _context;

    public HasAnomalieService(
        HttpClient httpClient,
        ILogger<HasAnomalieService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _ollamaUrl = configuration["Ollama:Url"] ?? "http://localhost:11434/api/generate";
        _model = configuration["Ollama:Model"] ?? "mistral";
        _context = configuration["Ollama:Context"] ?? "Você deve identificar se o conteúdo dessa página possui algum conteúdo relacionado à sexo.";
    }

    public async Task<List<ResultResponse>> AnalyzePageAsync(PageProcessedMessage page)
    {
        var results = new List<ResultResponse>();
        string responseString = "";
        foreach (var s in page.CrawlRequest.SubjectsToResearch)
        {
            responseString = "";
            try
            {
                _logger.LogInformation("Iniciando análise de {assunto} para URL: {Url}", s.Name, page.Url);

                var prompt = BuildAnalysisPrompt(page, s);

                var requestBody = new
                {
                    model = _model,
                    prompt = $"\n\n{prompt}",
                    stream = false,
                    options = new
                    {
                        num_ctx = 32768, // 32768
                        num_predict = 6048

                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(_ollamaUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Erro na chamada para Ollama: {StatusCode}", response.StatusCode);
                    results.Add(CreateErrorResult(page.CrawlRequest, s));
                    continue;
                }

                responseString = await response.Content.ReadAsStringAsync();
                var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseString);
                _logger.LogInformation(ollamaResponse?.response);
                var x = JsonSerializer.Deserialize<ResultResponse>(ollamaResponse?.response);
                x.CrawlRequest = page.CrawlRequest;
                x.SubjectsResearched = s;

                _logger.LogInformation("Análise concluída para {Url}. Anomalia detectada: {HasAnomalie}",
                    page.Url, x.HasAnomalie);

                results.Add(x);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante análise de anomalia para URL: {Url} e resposta {resposta}", page.Url, responseString);
                results.Add(CreateErrorResult(page.CrawlRequest, s));
                continue;
            }
        }

        return results;
    }

    private string ExtractOnlyRelevantText(string html)
    {
        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Remover scripts e estilos
            doc.DocumentNode.Descendants()
                .Where(n => n.Name == "script" || n.Name == "style")
                .ToList()
                .ForEach(n => n.Remove());

            // Extrair texto de parágrafos, headers e outros elementos relevantes
            var textos = new List<string>();

            // Parágrafos
            var paragrafos = doc.DocumentNode.SelectNodes("//p");
            if (paragrafos != null)
            {
                foreach (var p in paragrafos)
                {
                    var texto = p.InnerText.Trim();
                    if (!string.IsNullOrWhiteSpace(texto))
                    {
                        textos.Add(texto);
                    }
                }
            }

            // Headers (h1-h6)
            var headers = doc.DocumentNode.SelectNodes("//h1|//h2|//h3|//h4|//h5|//h6");
            if (headers != null)
            {
                foreach (var h in headers)
                {
                    var texto = h.InnerText.Trim();
                    if (!string.IsNullOrWhiteSpace(texto))
                    {
                        textos.Add(texto);
                    }
                }
            }

            // Artigos
            var articles = doc.DocumentNode.SelectNodes("//article");
            if (articles != null)
            {
                foreach (var article in articles)
                {
                    var texto = article.InnerText.Trim();
                    if (!string.IsNullOrWhiteSpace(texto) && !textos.Contains(texto))
                    {
                        textos.Add(texto);
                    }
                }
            }

            // Juntar todos os textos
            var resultado = string.Join("\n\n", textos);

            // Limpar espaços extras e quebras de linha múltiplas
            resultado = System.Text.RegularExpressions.Regex.Replace(resultado, @"\s+", " ");
            resultado = System.Text.RegularExpressions.Regex.Replace(resultado, @"\n{3,}", "\n\n");

            // Log do tamanho para debug
            _logger.LogInformation("Texto extraído: {TamanhoOriginal} -> {TamanhoLimpo} caracteres",
                html.Length, resultado.Length);

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao extrair texto do HTML");
            // Em caso de erro, retornar uma versão simplificada
            return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", " ");
        }
    }
    private string BuildAnalysisPrompt(PageProcessedMessage page, SubjectToResearch subject)
    {
        var description = !string.IsNullOrEmpty(subject.Description)
            ? $"({ subject.Description })"
            : "";

        /* Analise o texto abaixo e diga se ele menciona explicitamente PEDOFILIA
           (relações sexuais ou exploração sexual envolvendo menores de idade).

           - Se mencionar explicitamente: {"HasAnomalie": true, "Analysis": "Descreva brevemente o trecho"}
           - Se NÃO mencionar explicitamente: {"HasAnomalie": false, "Analysis": ""}

           Texto:
           -----------------
           {conteudo}
           -----------------
           Responda SOMENTE com o JSON.
           */
        var sb = new StringBuilder();
        sb.AppendLine("Instruções");
        sb.AppendLine($"Preciso que me diga se no texto abaixo há algo relacionado {subject.Name}{description}, sendo que você deve dizer se sim somente se o assunto de {subject.Name} estiver explícito no texto. ");
        sb.AppendLine($"------------------------");
        sb.AppendLine($"{ExtractOnlyRelevantText(page.HtmlContent)}");
        sb.AppendLine($"------------------------");
        sb.AppendLine($"Informe se há algo relacionado à {subject.Name}{description}. Foque somente no conteúdo principal da páginal e não em títulos de links ou chamadas para outros locais. Você responder que há anomalia exclusivamente se o assunto de {subject.Name} estiver presente no texto, não devendo falar sim se explicitamente não tiver.  Não deduza nada que não estiver no texto, por exemplo, não deduza que sexo a três seja algo relacionado com pedofilia se não estiver claro que um dos participantes é menor de idade.");
        sb.AppendLine($"Retorne um JSON com uma propriedade HasAnomalie - do tipo booleano - indicando SOMENTE SE O TEXTO TIVER {subject.Name}, e uma propriedade Analysis com a descrição da análise - SEMPRE EM PORTUGUÊS. Importante: SE NÃO TIVER EXPLICITO {subject.Name} NO TEXTO, O HASANOMALIES DEVE SER FALSE.  Não deve haver nenhuma explicação ou caracteres antes ou depois do json, ou seja, a sua resposta DEVE SER SOMENTE O JSON.");
        sb.AppendLine($"De forma alguma alguma misture assuntos, como por exemplo se o texto estiver falando de sexo com menores, mas o assunto pesquisado for racismo, você não deve retornar true, pois o assunto pesquisado não é racismo, mas sim sexo com menores. Você deve retornar false.");
        sb.AppendLine($"## Exemplos de resposta JSON");
        sb.AppendLine($"{{\"HasAnomalie\": true, \"Analysis\": \"bla bla bla\"}}\n");
        sb.AppendLine($"{{\"HasAnomalie\": false, \"Analysis\": \"\"}}\n");
        sb.AppendLine($"## Fim dos exemplos de resposta JSON");
        sb.AppendLine($"## NÃO ESQUEÇA");
        sb.AppendLine($"De forma alguma coloque algo antes ou depois do json, ou seja, a sua resposta DEVE SER SOMENTE O JSON. Não coloque, por exemplo, 'Aqui está a minha resposta:'.");
        return sb.ToString();
    }

    private ResultResponse CreateErrorResult(CrawlRequest request, SubjectToResearch subject)
    {
        return new ResultResponse
        {
            HasAnomalie = false,
            Analysis = "Erro durante análise - não foi possível determinar se há anomalias",
            SuccessAnalyze = false,
            AnalyzedAt = DateTime.UtcNow,
            CrawlRequest = request,
            SubjectsResearched = subject
        };
    }

    private class OllamaResponse
    {
        public string model { get; set; }
        public string created_at { get; set; }
        public string response { get; set; }
        public bool done { get; set; }
        public string done_reason { get; set; }
        public List<long> context { get; set; }
        public long total_duration { get; set; }
        public long load_duration { get; set; }
        public int prompt_eval_count { get; set; }
        public long prompt_eval_duration { get; set; }
        public int eval_count { get; set; }
        public long eval_duration { get; set; }


    }



}
