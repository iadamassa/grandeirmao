# 🛡️ Monitor de Anomalias - Sistema de Detecção Inteligente

Sistema completo para **monitoramento de conteúdo web** com detecção automatizada de anomalias usando **Inteligência Artificial**. O sistema combina um **painel de controle web**, **crawler inteligente** e **motor de análise com IA** para identificar conteúdo problemático em sites.

## 📋 Visão Geral do Sistema

O **Monitor de Anomalias** é uma solução completa que:

- **Monitora sites** de forma automatizada
- **Detecta conteúdo problemático** usando IA (Ollama/LLM)
- **Classifica e categoriza** anomalias encontradas
- **Fornece interface** para configuração e visualização
- **Processa em tempo real** usando filas RabbitMQ

## 🏗️ Arquitetura do Sistema

O projeto é composto por **4 componentes principais** que se comunicam através de **mensageria assíncrona** (RabbitMQ), permitindo **distribuição horizontal** em múltiplas máquinas para alta performance e escalabilidade.

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   PAINEL WEB    │    │    CRAWLER      │    │  FINDANOMALIE   │    │    RABBITMQ     │
│  (Interface)    │    │  (Extração)     │    │   (Análise)     │    │   (Mensageria)  │
│                 │    │                 │    │                 │    │                 │
│ • Dashboard     │───▶│ • HTTP Client   │───▶│ • Ollama/LLM    │───▶│ • Filas         │
│ • Configuração  │    │ • HTML Parser   │    │ • Classificação │    │ • Exchange      │
│ • Relatórios    │    │ • Link Extract  │    │ • Detecção IA   │    │ • Routing       │
└─────────────────┘    └─────────────────┘    └─────────────────┘    └─────────────────┘
```

### 🌐 Arquitetura Distribuída

O sistema foi projetado para **processamento distribuído**, onde cada componente pode ser executado em **máquinas diferentes**, conectando-se ao **RabbitMQ central** para coordenação. Esta arquitetura oferece:

#### ✅ **Escalabilidade Horizontal**
- **Múltiplas instâncias** de cada serviço em máquinas diferentes
- **Load balancing** automático via filas RabbitMQ
- **Adição dinâmica** de novos nós conforme demanda

#### ✅ **Alta Disponibilidade**
- **Tolerância a falhas**: Se uma máquina falhar, outras continuam processando
- **Redundância**: Múltiplas instâncias dos mesmos serviços
- **Recovery automático**: Mensagens não processadas são redistribuídas

#### ✅ **Performance Otimizada**
- **Processamento paralelo**: Múltiplos crawlers e analisadores simultaneamente
- **Especialização por hardware**: IA em GPUs, crawling em CPUs otimizadas
- **Balanceamento inteligente** de carga entre nós

### 🔄 Comunicação via Mensageria

Todos os componentes se comunicam **exclusivamente através do RabbitMQ**, eliminando dependências diretas e permitindo distribuição:

```
🏢 MÁQUINA 1 - Controle           🏢 MÁQUINA 2 - Crawling          🏢 MÁQUINA 3 - Análise IA
┌─────────────────────┐          ┌─────────────────────┐          ┌─────────────────────┐
│     PAINEL WEB      │          │     CRAWLER #1      │          │   FINDANOMALIE #1   │
│                     │          │                     │          │                     │
│ • Interface Admin   │          │ • Extração Web      │          │ • Análise Ollama    │
│ • Dashboard         │          │ • HTML Parsing      │          │ • Detecção IA       │
│ • PostgreSQL        │          │                     │          │ • GPU Dedicada      │
└─────────┬───────────┘          └─────────┬───────────┘          └─────────┬───────────┘
          │                                │                                │
          │                                │                                │
    ┌─────▼──────────────────────────────────▼────────────────────────────▼─────┐
    │                           RABBITMQ CENTRAL                                │
    │                        🏢 MÁQUINA 4 - Mensageria                         │
    │                                                                           │
    │  📨 crawl-requests              📨 pages-processed       📨 links-analysed │
    │  ├─ Queue: Trabalhos            ├─ Queue: Páginas       ├─ Queue: Results │  
    │  ├─ Routing: por prioridade     ├─ Load Balance         ├─ Persistence     │
    │  └─ Multiple consumers          └─ Múltiplos workers    └─ Ack garantido   │
    └───────────────────────────────────────────────────────────────────────────┘
          ▲                                ▲                                ▲
          │                                │                                │
┌─────────┴───────────┐          ┌─────────┴───────────┐          ┌─────────┴───────────┐
│     CRAWLER #2      │          │   FINDANOMALIE #2   │          │   FINDANOMALIE #3   │
│                     │          │                     │          │                     │
│ • Backup Instance   │          │ • CPU Instance      │          │ • Especializada     │
│ • Load Balancer     │          │ • Modelo Pequeno    │          │ • Modelo Grande     │
│ 🏢 MÁQUINA 5        │          │ 🏢 MÁQUINA 6        │          │ 🏢 MÁQUINA 7        │
└─────────────────────┘          └─────────────────────┘          └─────────────────────┘
```

### 📊 **Fluxo de Processamento Distribuído**

1. **📝 Configuração** (Painel Web)
   - Admin configura **sites e assuntos** para monitoramento
   - Sistema **publica mensagens** `CrawlRequest` na fila
   - **Qualquer crawler disponível** na rede pode processar

2. **🕷️ Crawling Distribuído** (Múltiplos Crawlers)
   - Crawlers em **diferentes máquinas** consomem fila `crawl-requests`
   - **Load balancing automático**: próximo crawler disponível pega o trabalho  
   - Cada crawler processa **independentemente** e publica `PageProcessedMessage`
   - **Failover**: Se um crawler falha, outros continuam processando

3. **🧠 Análise IA Distribuída** (Múltiplos FindAnomalies)
   - Instâncias de IA em **máquinas especializadas** (GPU, CPU otimizada)
   - **Especialização por modelo**: Mistral em máquina A, Llama3 em máquina B
   - **Balanceamento inteligente**: Análises simples → CPU, complexas → GPU
   - Cada resultado é **publicado de volta** para o Painel

4. **📈 Consolidação** (Painel Web)
   - **Único painel centralizado** recebe todos os resultados
   - **Dashboard unificado** mostra progresso de todas as máquinas
   - **Banco PostgreSQL** centralizado com todos os dados

### ⚙️ **Configuração para Distribuição**

Para executar componentes em **máquinas diferentes**, altere apenas o **endereço do RabbitMQ**:

```bash
# MÁQUINA 1 - Painel Web + PostgreSQL + RabbitMQ
docker-compose up painel postgres rabbitmq redis

# MÁQUINA 2 - Crawlers (2 instâncias)
export RABBITMQ_HOST=ip-da-maquina-1
docker-compose up --scale crawler=2 crawler

# MÁQUINA 3 - FindAnomalie GPU (1 instância com GPU)  
export RABBITMQ_HOST=ip-da-maquina-1
docker-compose up findanomalie-gpu

# MÁQUINA 4 - FindAnomalie CPU (3 instâncias CPU)
export RABBITMQ_HOST=ip-da-maquina-1  
docker-compose up --scale findanomalie=3 findanomalie
```

### 🎯 **Benefícios da Arquitetura Distribuída**

- **⚡ Performance**: Processamento paralelo em múltiplas máquinas
- **📈 Escalabilidade**: Adicione máquinas conforme crescimento
- **🛡️ Resiliência**: Falha de uma máquina não para o sistema
- **💰 Otimização de Custo**: Use hardware especializado (GPU para IA, CPU para crawling)
- **🔧 Manutenção**: Atualize componentes sem parar o sistema completo

### Fluxo de Processamento

```mermaid
graph LR
    A[Usuário configura sites] --> B[Painel envia para fila]
    B --> C[Crawler processa URLs]
    C --> D[Extrai conteúdo HTML]
    D --> E[Envia para análise]
    E --> F[IA analisa conteúdo]
    F --> G[Detecta anomalias]
    G --> H[Salva resultados]
    H --> I[Atualiza dashboard]
```

## 🔧 Componentes Detalhados

### 1. 📊 **Painel Web** (Interface de Gestão)

**Localização:** `/Painel/`

Interface web completa para gestão e monitoramento do sistema.

**Tecnologias:**
- **Backend**: .NET 8, Entity Framework Core, MediatR (CQRS)
- **Frontend**: React 18, TypeScript, Tailwind CSS
- **Banco**: PostgreSQL (produção) / SQLite (desenvolvimento)
- **Cache**: Redis
- **Autenticação**: JWT + ASP.NET Identity

**Funcionalidades:**
- ✅ **Dashboard** com gráficos e métricas em tempo real
- ✅ **Gestão de Sites** para monitoramento
- ✅ **Categorias** para classificação de sites
- ✅ **Assuntos de Pesquisa** para configurar detecções
- ✅ **Relatórios de Anomalias** com filtros avançados
- ✅ **Exportação para Excel** dos resultados
- ✅ **Sistema de usuários** com autenticação segura

**Como executar:**
```bash
# Backend (.NET)
cd Painel/painel/api/AnomaliaMonitor.Solution/WebAPI/AnomaliaMonitor.WebAPI
dotnet run

# Frontend (React)
cd Painel/painel/frontend
npm install && npm start
```

**Acesso:** http://localhost:3000 (frontend) / https://localhost:7190 (backend)

---

### 2. 🕷️ **Crawler** (Extração de Conteúdo)

**Localização:** `/crawler/`

Serviço responsável por navegar e extrair conteúdo dos sites configurados.

**Tecnologias:**
- **.NET 8** com HttpClient otimizado
- **HtmlAgilityPack** para parsing de HTML
- **MassTransit** para integração com RabbitMQ
- **Semáforos** para controle de concorrência

**Funcionalidades:**
- 🔍 **Crawling inteligente** com simulação de comportamento humano
- 📄 **Extração de metadados** (título, descrição, keywords)
- 🔗 **Descoberta automática** de links internos
- 📤 **Publicação automática** de páginas processadas
- ⚡ **Processamento assíncrono** com controle de rate limiting
- 🛡️ **Headers dinâmicos** para evitar bloqueios

**Processo de funcionamento:**
1. **Recebe** mensagem `CrawlRequest` via RabbitMQ
2. **Extrai** conteúdo HTML da URL solicitada
3. **Descobre** links internos automaticamente
4. **Processa** cada página encontrada
5. **Publica** `PageProcessedMessage` para análise

**Como executar:**
```bash
cd crawler
dotnet restore
dotnet run
```

**Filas utilizadas:**
- **Entrada**: `crawl-requests` (recebe URLs para processar)
- **Saída**: `CrawlerWebApi.Models:PageProcessedMessage` (páginas processadas)

---

### 3. 🧠 **FindAnomalie** (Detecção com IA)

**Localização:** `/findanomalie/`

Motor de análise que utiliza Inteligência Artificial para detectar conteúdo problemático.

**Tecnologias:**
- **.NET 8** para processamento
- **Ollama** (LLM local) para análise inteligente
- **HtmlAgilityPack** para limpeza de conteúdo
- **MassTransit** para comunicação assíncrona

**Funcionalidades:**
- 🤖 **Análise com IA** usando modelos Ollama (Mistral, Llama3, etc.)
- 🎯 **Detecção especializada** por assunto configurado
- 📊 **Classificação de confiança** com scoring
- 🧹 **Limpeza de HTML** para análise focada
- 📝 **Relatórios detalhados** da análise
- 🔄 **Processamento em lote** de múltiplos assuntos

**Tipos de detecção configuráveis:**
- Conteúdo inadequado para menores
- Material relacionado a crimes
- Spam e phishing
- Conteúdo discriminatório
- Violação de direitos autorais
- Outros conforme configuração

**Processo de funcionamento:**
1. **Recebe** `PageProcessedMessage` do crawler
2. **Limpa** e extrai texto relevante do HTML
3. **Analisa** conteúdo usando IA/LLM configurado
4. **Classifica** anomalias por assunto pesquisado
5. **Publica** `LinkAnalysedMessage` com resultados

**Como executar:**
```bash
cd findanomalie
dotnet restore
dotnet run

# Requer Ollama rodando
ollama serve
ollama pull mistral  # ou llama3
```

---

### 4. 🐰 **RabbitMQ** (Sistema de Mensageria)

**Papel:** Comunicação assíncrona entre todos os componentes.

**Filas e mensagens:**

```
📨 FLUXO DE MENSAGENS:

1. crawl-requests
   ├─ Tipo: CrawlRequest
   ├─ Origem: Painel Web
   ├─ Destino: Crawler
   └─ Conteúdo: { url, requestId, siteId, subjectsToResearch[] }

2. CrawlerWebApi.Models:PageProcessedMessage  
   ├─ Tipo: PageProcessedMessage
   ├─ Origem: Crawler
   ├─ Destino: FindAnomalie
   └─ Conteúdo: { url, title, htmlContent, metaData, crawledAt }

3. CheckAnomaliaApi.Models:LinkAnalysedMessage
   ├─ Tipo: LinkAnalysedMessage
   ├─ Origem: FindAnomalie  
   ├─ Destino: Painel Web
   └─ Conteúdo: { hasAnomalie, analysis, confidence, subject }
```

**Configuração padrão:**
- **Host:** localhost:5672
- **Usuário:** guest/guest
- **Management UI:** http://localhost:15672

---

## 🚀 Configuração e Execução Completa

### 🐳 Execução com Docker Compose (Recomendado)

**Pré-requisitos:**
- Docker e Docker Compose instalados

**Execução rápida:**
```bash
# Clone o repositório
git clone <repository-url>
cd "Monitor Anomalias"

# Executar todos os serviços
docker-compose up -d

# Verificar status dos containers
docker-compose ps

# Acompanhar logs
docker-compose logs -f
```

**Serviços disponíveis após execução:**
- **Frontend**: http://localhost:9003
- **API**: http://localhost:9000
- **Crawler**: http://localhost:9001  
- **FindAnomalie**: http://localhost:9002
- **RabbitMQ Management**: http://localhost:15672 (admin/admin123)
- **PostgreSQL**: localhost:5432 (postgres/postgres123)
- **Redis**: localhost:6379

**Comandos úteis:**
```bash
# Parar todos os serviços
docker-compose down

# Remover volumes (dados)
docker-compose down -v

# Rebuild de um serviço específico
docker-compose build api
docker-compose up -d api

# Ver logs de um serviço
docker-compose logs -f crawler
```

### 📋 Execução Manual (Desenvolvimento)

### Pré-requisitos

```bash
# Instalar dependências
- .NET 8 SDK
- Node.js 18+
- PostgreSQL Server
- RabbitMQ Server
- Redis Server
- Ollama (para IA)

# Instalar PostgreSQL
sudo apt install postgresql postgresql-contrib  # Ubuntu/Debian
brew install postgresql                         # macOS

# Instalar RabbitMQ
sudo apt install rabbitmq-server    # Ubuntu/Debian
brew install rabbitmq               # macOS

# Instalar Redis
sudo apt install redis-server       # Ubuntu/Debian
brew install redis                  # macOS

# Instalar Ollama
curl -fsSL https://ollama.com/install.sh | sh
ollama pull mistral  # ou llama3
```

### Ordem de Execução

```bash
# 1. Iniciar serviços de infraestrutura
sudo systemctl start postgresql
sudo systemctl start rabbitmq-server
sudo systemctl start redis-server

# 2. Iniciar Ollama
ollama serve

# 3. Configurar banco PostgreSQL
sudo -u postgres createdb anomalia_monitor

# 4. Iniciar componentes (.NET)
cd crawler && dotnet run &
cd findanomalie && dotnet run &
cd Painel/api/WebAPI/AnomaliaMonitor.WebAPI && dotnet run &

# 5. Iniciar frontend React
cd Painel/frontend && npm install && npm start
```

### Verificação dos Serviços

```bash
# Docker Compose
curl http://localhost:9000/api/test            # Painel API
curl http://localhost:9001/health              # Crawler
curl http://localhost:9002/health              # FindAnomalie
curl http://localhost:9003                     # Frontend

# Execução Manual
curl http://localhost:5000/api/crawler/health  # Crawler
curl http://localhost:8080/health              # FindAnomalie  
curl https://localhost:7190/api/test           # Painel API
curl http://localhost:3000                     # Frontend

# RabbitMQ Management
open http://localhost:15672
```

## 📊 Uso do Sistema

### 1. **Configuração Inicial**

1. **Acesse** o painel web em:
   - Docker Compose: http://localhost:9003
   - Execução manual: http://localhost:3000
2. **Faça login** com: `teste@teste.com` / `Teste123`
3. **Configure assuntos** a serem pesquisados
4. **Adicione categorias** de sites
5. **Cadastre sites** para monitoramento

### 2. **Iniciando Monitoramento**

1. **Selecione um site** cadastrado
2. **Clique em "Iniciar Crawling"**
3. **Acompanhe o progresso** no dashboard
4. **Visualize resultados** na aba "Anomalias"

### 3. **Análise de Resultados**

- **Dashboard**: Gráficos e métricas gerais
- **Anomalias**: Lista detalhada com filtros
- **Relatórios**: Exportação para Excel
- **Sites**: Status de cada site monitorado

## 🔧 Configurações Avançadas

### Configuração do Ollama

```json
// findanomalie/appsettings.json
{
  "Ollama": {
    "Url": "http://localhost:11434/api/generate",
    "Model": "mistral",  // ou "llama3"
    "Context": "Contexto personalizado para análise..."
  }
}
```

### Configuração do RabbitMQ

```json
// Em todos os appsettings.json
{
  "ConnectionStrings": {
    "RabbitMQ": "amqp://usuario:senha@localhost:5672/"
  }
}
```

### Configuração do Crawler

```csharp
// crawler/Services/CrawlerService.cs
private const int MaxConcurrency = 5;     // URLs simultâneas
private const int MinDelayMs = 500;       // Delay mínimo
private const int MaxDelayMs = 3000;      // Delay máximo
```

## 📈 Monitoramento e Logs

### Logs do Sistema

Cada componente gera logs detalhados:

```bash
# Crawler
🚀 Iniciando extração completa para: https://example.com
🔍 Extraindo dados de: https://example.com/page1
📤 Página enviada para fila de processamento

# FindAnomalie
📄 Recebida página processada: https://example.com/page1
🔍 Análise concluída - Anomalia detectada: true
🎯 Confiança: 85.50%

# Painel
📨 LinkAnalysedMessage recebida para URL: https://example.com/page1
💾 Anomalia salva no banco de dados
```

### Métricas Disponíveis

- **Taxa de processamento** (páginas/minuto)
- **Taxa de detecção** (anomalias encontradas)
- **Performance dos modelos** (tempo de análise)
- **Status das filas** RabbitMQ
- **Estatísticas de crawling** por site

## 🔒 Segurança

### Características de Segurança

- ✅ **Autenticação JWT** para acesso ao painel
- ✅ **Validação de entrada** em todos os endpoints
- ✅ **Rate limiting** no crawler para evitar sobrecarga
- ✅ **Sanitização HTML** antes da análise IA
- ✅ **Logs auditáveis** de todas as operações
- ✅ **Comunicação criptografada** entre componentes

### Boas Práticas Implementadas

- **User-Agent rotation** para crawler
- **Headers dinâmicos** simulando navegador real
- **Delays aleatórios** entre requisições
- **Retry policies** configuráveis
- **Timeout controls** em todas as operações

## 🛠️ Desenvolvimento e Extensões

### Estrutura de Diretórios

```
Monitor Anomalias/
├── Painel/                          # Interface Web
│   ├── painel/api/                 # Backend .NET
│   └── painel/frontend/            # Frontend React
├── crawler/                        # Serviço de Crawling
│   ├── Services/                   # Lógica de extração
│   └── Consumers/                  # Consumers RabbitMQ
├── findanomalie/                   # Motor de Análise IA
│   ├── Services/                   # Serviços de análise
│   └── Consumers/                  # Consumers RabbitMQ
└── README.md                       # Esta documentação
```

### Adicionando Novos Tipos de Análise

1. **Crie novo assunto** no painel web
2. **Configure exemplos** específicos do assunto
3. **Ajuste prompt** no `HasAnomalieService.cs`
4. **Teste com páginas** conhecidas

### Integrando Novos Modelos IA

```csharp
// findanomalie/appsettings.json
{
  "Ollama": {
    "Model": "novo-modelo",
    "Url": "http://outro-servidor:11434/api/generate"
  }
}
```

## 📝 Troubleshooting

### Problemas Comuns

**RabbitMQ não conecta:**
```bash
sudo systemctl status rabbitmq-server
sudo systemctl restart rabbitmq-server
```

**Ollama não responde:**
```bash
ollama list                    # Ver modelos instalados
ollama pull mistral            # Baixar modelo
curl http://localhost:11434    # Testar conexão
```

**Crawler não processa:**
- Verificar filas no RabbitMQ Management
- Conferir logs do serviço
- Validar URLs configuradas

**IA não detecta anomalias:**
- Conferir modelo Ollama carregado
- Verificar prompts configurados
- Testar com exemplos conhecidos

## 🤝 Contribuição

Para contribuir com o projeto:

1. **Fork** o repositório
2. **Crie branch** para feature (`git checkout -b feature/nova-funcionalidade`)
3. **Commit** mudanças (`git commit -m 'Add nova funcionalidade'`)
4. **Push** para branch (`git push origin feature/nova-funcionalidade`)
5. **Abra Pull Request**

## 📄 Licença

Este projeto está sob licença **MIT**. Veja o arquivo `LICENSE` para detalhes.

## 🆘 Suporte

Para dúvidas ou problemas:

- **Issues**: Abra uma issue no repositório
- **Documentação**: Consulte os READMEs específicos de cada componente
- **Logs**: Sempre consulte os logs dos serviços para diagnóstico

---

**🛡️ Desenvolvido para monitoramento defensivo e identificação de conteúdo problemático na web.**