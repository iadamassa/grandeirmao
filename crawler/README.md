# Crawler Web API

Web API em .NET 8 que recebe mensagens do RabbitMQ via MassTransit para executar crawling de sites.

## Características

- ✅ Processa apenas uma mensagem por vez (concorrência limitada)
- ✅ Integração com RabbitMQ via MassTransit
- ✅ Retry policy configurado para falhas
- ✅ Crawler completo com extração de metadados
- ✅ Logs detalhados do processamento
- ✅ Simulação de comportamento humano (delays, headers)
- ✅ **Publicação automática de páginas processadas com sucesso**

## Pré-requisitos

- .NET 8 SDK
- RabbitMQ rodando (padrão: localhost:5672)

## Como executar

1. **Instalar dependências**:
```bash
dotnet restore
```

2. **Executar a aplicação**:
```bash
dotnet run
```

3. **Verificar saúde da API**:
```bash
curl http://localhost:5000/api/crawler/health
```

## Como usar

### Enviando mensagem via API (para testes)

```bash
curl -X POST http://localhost:5000/api/crawler/crawl \
  -H "Content-Type: application/json" \
  -d '{"url": "https://example.com"}'
```

### Enviando mensagem diretamente para RabbitMQ

Publique uma mensagem JSON na fila `crawl-requests`:

```json
{
  "url": "https://example.com",
  "requestedAt": "2024-01-01T00:00:00Z",
  "requestId": "guid-aqui"
}
```

## Filas do RabbitMQ

A aplicação trabalha com **duas filas** principais:

### 1. Fila de Entrada: `crawl-requests`
**Propósito**: Recebe solicitações de crawling  
**Formato da mensagem**:
```json
{
  "url": "https://example.com",
  "requestedAt": "2024-01-01T00:00:00Z",
  "requestId": "12345678-1234-1234-1234-123456789abc"
}
```

### 2. Fila de Saída: `page-processed` (Exchange automático do MassTransit)
**Propósito**: Recebe dados das páginas processadas com sucesso  
**Nome da fila no RabbitMQ**: `CrawlerWebApi.Models:PageProcessedMessage`  
**Formato da mensagem**:
```json
{
  "url": "https://example.com/pagina",
  "title": "Título da Página",
  "htmlContent": "<html>...</html>",
  "metaDescription": "Descrição da página extraída do meta tag",
  "metaKeywords": "palavra1, palavra2, palavra3",
  "contentSize": 15420,
  "statusCode": 200,
  "contentType": "text/html",
  "internalLinksCount": 25,
  "crawledAt": "2024-01-01T10:30:00.000Z",
  "processedAt": "2024-01-01T10:30:05.250Z"
}
```

## Consumindo a fila de páginas processadas

### Via RabbitMQ Management Interface
1. Acesse: `http://localhost:15672`
2. Vá em **Queues and Streams**
3. Procure pela fila: `CrawlerWebApi.Models:PageProcessedMessage`
4. Clique em **Get Messages** para visualizar

### Via MassTransit Consumer (C#)
```csharp
public class PageProcessedConsumer : IConsumer<PageProcessedMessage>
{
    private readonly ILogger<PageProcessedConsumer> _logger;

    public PageProcessedConsumer(ILogger<PageProcessedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PageProcessedMessage> context)
    {
        var message = context.Message;
        
        _logger.LogInformation("📄 Nova página processada: {Url}", message.Url);
        _logger.LogInformation("🏷️  Título: {Title}", message.Title);
        _logger.LogInformation("📊 Tamanho: {Size} chars, Links: {Links}", 
                             message.ContentSize, message.InternalLinksCount);
        
        await ProcessPageContent(message);
    }

    private async Task ProcessPageContent(PageProcessedMessage page)
    {
        // Sua lógica de processamento aqui
        await Task.CompletedTask;
    }
}
```

 
## Configuração

### appsettings.json

```json
{
  "ConnectionStrings": {
    "RabbitMQ": "amqp://usuario:senha@localhost:5672/"
  }
}
```

### Variáveis de ambiente

- `ConnectionStrings__RabbitMQ`: String de conexão do RabbitMQ

## Estrutura do projeto

```
├── Controllers/
│   └── CrawlerController.cs           # API para testes
├── Consumers/
│   └── CrawlRequestConsumer.cs        # Consumer MassTransit
├── Models/
│   ├── CrawlRequest.cs                # Modelo da mensagem de entrada
│   ├── CrawlResult.cs                 # Resultado do crawling
│   ├── PageData.cs                    # Dados de uma página
│   └── PageProcessedMessage.cs        # Mensagem de página processada
├── Services/
│   ├── ICrawlerService.cs             # Interface do serviço
│   └── CrawlerService.cs              # Implementação do crawler
├── Program.cs                         # Configuração da aplicação
└── appsettings.json                  # Configurações
```

## Funcionamento

1. A aplicação se conecta ao RabbitMQ e fica ouvindo a fila `crawl-requests`
2. Quando recebe uma mensagem com uma URL, inicia o crawling
3. O semáforo garante que apenas uma operação de crawling aconteça por vez
4. **Para cada página processada com sucesso**, uma mensagem é publicada na fila `page-processed`
5. A mensagem contém URL, título, conteúdo HTML e metadados extraídos
6. Em caso de erro, utiliza a política de retry configurada

## Dados da mensagem PageProcessedMessage

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `url` | string | URL da página processada |
| `title` | string | Título extraído da tag `<title>` |
| `htmlContent` | string | Conteúdo HTML completo da página |
| `metaDescription` | string | Conteúdo da meta tag description |
| `metaKeywords` | string | Conteúdo da meta tag keywords |
| `contentSize` | int | Tamanho do HTML em caracteres |
| `statusCode` | int | Status HTTP da resposta (200, 404, etc.) |
| `contentType` | string | Content-Type da resposta |
| `internalLinksCount` | int | Quantidade de links internos encontrados |
| `crawledAt` | DateTime | Data/hora da extração da página |
| `processedAt` | DateTime | Data/hora do envio da mensagem |

## Logs

A aplicação gera logs detalhados sobre:
- Recebimento de mensagens
- Progresso do crawling
- **Envio de páginas para fila de processamento**
- Estatísticas finais
- Erros e tentativas de retry

## Casos de uso para a fila de páginas processadas

- **Indexação**: Enviar conteúdo para Elasticsearch ou Solr
- **Análise de conteúdo**: Processar HTML com IA/ML
- **Armazenamento**: Salvar páginas em banco de dados
- **Notificações**: Alertar sobre páginas específicas
- **Backup**: Arquivar conteúdo processado
- **Analytics**: Extrair métricas e estatísticas

## Personalização

Para salvar os resultados em banco de dados ou enviar para outras filas/exchanges, modifique o método `ProcessSuccessfulPageAsync` no `CrawlerService` ou crie consumers específicos para a fila `PageProcessedMessage`.
```
