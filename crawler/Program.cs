using MassTransit;
using CrawlerWebApi.Services;
using CrawlerWebApi.Consumers;
using CrawlerWebApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar HttpClient para o CrawlerService com User-Agents aleatórios
builder.Services.AddHttpClient("CrawlerClient", client =>
{
    // Array de User-Agents do Firefox em diferentes sistemas operacionais
    var userAgents = new[]
    {
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:121.0) Gecko/20100101 Firefox/121.0",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:120.0) Gecko/20100101 Firefox/120.0",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:121.0) Gecko/20100101 Firefox/121.0",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
        "Mozilla/5.0 (X11; Linux x86_64; rv:121.0) Gecko/20100101 Firefox/121.0",
        "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:121.0) Gecko/20100101 Firefox/121.0"
    };

    // Seleciona um User-Agent aleatório
    var random = new Random();
    var selectedUserAgent = userAgents[random.Next(userAgents.Length)];

    // Configura o User-Agent selecionado
    client.DefaultRequestHeaders.UserAgent.TryParseAdd(selectedUserAgent);

    // Headers completos para simulação realista de navegador
    client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
    client.DefaultRequestHeaders.Add("Accept-Language", "pt-BR,pt;q=0.8,en;q=0.5,en-US;q=0.3");
    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
    client.DefaultRequestHeaders.Add("DNT", "1");
    client.DefaultRequestHeaders.Add("Connection", "keep-alive");
    client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
    client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
    client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
    client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
    client.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
    client.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");

    // Configurações de timeout e comportamento
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Registrar serviços
builder.Services.AddScoped<ICrawlerService, CrawlerService>();

// Configurar MassTransit com RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Registrar o consumer
    x.AddConsumer<CrawlRequestConsumer>(cfg =>
    {
        // Configurar para processar apenas uma mensagem por vez
        cfg.UseConcurrentMessageLimit(1);
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        // Configuração da conexão RabbitMQ
        var uri = new Uri(builder.Configuration.GetConnectionString("RabbitMQ")
                          ?? "amqp://admin:admin123@rabbitmq:5672/");

        cfg.Host(uri);

        // Configurar a fila para o consumer
        cfg.ReceiveEndpoint("crawl-requests", e =>
        {
            // Configurar para processar apenas uma mensagem por vez
            e.PrefetchCount = 1;
            e.ConcurrentMessageLimit = 1;

            // Configurar retry policy
            e.UseMessageRetry(r => r.Intervals(
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30),
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(5)
            ));

            e.ConfigureConsumer<CrawlRequestConsumer>(context);
        });

        cfg.Message<PageProcessedMessage>(x =>
        {
            x.SetEntityName("page-processed-queue"); // Nome customizado da fila
        });



    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Log da inicialização
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("🚀 CrawlerWebApi iniciada em {Time}", DateTime.UtcNow);
logger.LogInformation("🐰 Aguardando mensagens do RabbitMQ na fila 'crawl-requests'");

app.Run();
