# 🔍 Sistema de Monitoramento de Anomalias

Sistema completo para catalogar e monitorar anomalias encontradas em sites, organizando por assuntos e categorias.

## 🏗️ Arquitetura

### Backend (.NET 8)
- **Clean Architecture** com separação em camadas
- **Domain Driven Design (DDD)**
- **CQRS** com MediatR
- **Entity Framework Core 8**
- **ASP.NET Identity** para autenticação
- **JWT** para autorização
- **FluentValidation** para validações
- **AutoMapper** para mapeamento de objetos

### Frontend (React 18)
- **TypeScript** para tipagem estática
- **Tailwind CSS** para estilização
- **React Router** para navegação
- **React Hook Form** com validação
- **Recharts** para gráficos
- **Lucide React** para ícones
- **Axios** para requisições HTTP

## 🗄️ Modelo de Dados

### Entidades Principais

1. **Assuntos a Pesquisar (SubjectToResearch)**
   - Temas/tópicos de interesse para monitoramento
   - Relaciona com exemplos e anomalias

2. **Exemplos de Assuntos (SubjectExample)**
   - Exemplos específicos de cada assunto
   - Ajudam na identificação de anomalias

3. **Categorias de Sites (SiteCategory)**
   - Classificação dos sites por tipo
   - Relacionamento N:N com assuntos e sites

4. **Sites**
   - Sites a serem monitorados
   - Contêm links específicos para análise

5. **Links dos Sites (SiteLink)**
   - URLs específicas dentro dos sites
   - Onde as anomalias são encontradas

6. **Anomalias (Anomaly)**
   - Registro das anomalias encontradas
   - Vinculadas a links e assuntos

## 🚀 Configuração e Execução

### Pré-requisitos

- **.NET 8 SDK**
- **Node.js 18+**
- **SQLite** (banco local)
- **Visual Studio 2022** ou **VS Code** (opcional)

### 1. Configuração do Backend

```bash
# Navegue para o diretório da solução
cd AnomaliaMonitor.Solution

# Restaure os pacotes NuGet
dotnet restore

# O banco SQLite será criado automaticamente
# Por padrão usa SQLite local (AnomaliaMonitor.db)

# Execute as migrações para criar o banco
cd WebAPI/AnomaliaMonitor.WebAPI
dotnet ef database update --project ../../Infrastructure/AnomaliaMonitor.Infrastructure

# Execute o backend
dotnet run
```

O backend estará disponível em: `https://localhost:7190`

### 2. Configuração do Frontend

```bash
# Navegue para o diretório do frontend
cd frontend

# Instale as dependências
npm install

# Configure a URL da API no arquivo .env se necessário
# Por padrão aponta para https://localhost:7190/api

# Execute o frontend
npm start
```

O frontend estará disponível em: `http://localhost:3000`

## 🔐 Credenciais de Acesso

### Usuário Padrão (criado automaticamente)
- **Email:** teste@teste.com
- **Senha:** Teste123

## 📊 Funcionalidades

### Dashboard
- **Cards de resumo:** Totais de Assuntos, Categorias, Sites e Anomalias
- **Gráfico temporal:** Anomalias por período com filtro de data
- **Top 10 Sites:** Sites com mais anomalias no período
- **Top 10 Assuntos:** Assuntos mais encontrados no período

### Gestão de Dados
- **CRUD completo** para todas as entidades
- **Filtros por período** em telas com datas
- **Validação** em formulários
- **Relacionamentos visuais** (dropdowns, multi-select)
- **Tabelas** com paginação e busca

### Autenticação
- **Login/Logout** 
- **Recuperação de senha** via email
- **JWT Tokens** para segurança
- **Proteção de rotas**

## 🎨 Design

### Tema Visual
- **Gradient azul para roxo** como tema principal
- **Glassmorphism** nos componentes
- **Design responsivo** para mobile/desktop
- **Animações suaves** e transições
- **Dark theme** moderno

### Componentes
- **Layout responsivo** com sidebar
- **Cards com glassmorphism**
- **Formulários validados**
- **Notificações toast**
- **Loading states**

## 🔧 Estrutura do Projeto

```
AnomaliaMonitor.Solution/
├── Domain/                          # Camada de Domínio
│   ├── Entities/                   # Entidades do negócio
│   ├── Interfaces/                 # Contratos
│   └── Common/                     # Classes base
├── Application/                     # Camada de Aplicação
│   ├── Features/                   # Commands e Queries
│   ├── DTOs/                       # Data Transfer Objects
│   └── Common/                     # Mapeamentos
├── Infrastructure/                  # Camada de Infraestrutura
│   ├── Data/                       # Contexto EF e configurações
│   └── Repositories/               # Implementações de repositório
├── WebAPI/                         # Camada de Apresentação
│   ├── Controllers/                # Controllers da API
│   └── Program.cs                  # Configuração da aplicação
└── frontend/                       # Frontend React
    ├── src/
    │   ├── components/            # Componentes reutilizáveis
    │   ├── pages/                 # Páginas da aplicação
    │   ├── services/              # Serviços API
    │   ├── types/                 # Definições TypeScript
    │   └── App.tsx                # Componente principal
    └── public/                    # Arquivos estáticos
```

## 🗃️ Banco de Dados

### Configuração
- **Provider:** SQLite
- **Migrations:** Entity Framework Core
- **Seed:** Dados iniciais criados automaticamente

### String de Conexão Padrão
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=AnomaliaMonitor.db"
  }
}
```

### Migrações
```bash
# Adicionar nova migração
dotnet ef migrations add NomeDaMigracao --project Infrastructure/AnomaliaMonitor.Infrastructure --startup-project WebAPI/AnomaliaMonitor.WebAPI

# Aplicar migrações
dotnet ef database update --project Infrastructure/AnomaliaMonitor.Infrastructure --startup-project WebAPI/AnomaliaMonitor.WebAPI
```

## 🔒 Segurança

### Configurações JWT
```json
{
  "JwtSettings": {
    "Secret": "MySecretKeyForJWT1234567890ABCDEFGHIJKLMNOP",
    "Issuer": "AnomaliaMonitor",
    "Audience": "AnomaliaMonitorUsers",
    "ExpiryDays": "7"
  }
}
```

### Recursos de Segurança
- **Tokens JWT** com expiração
- **Validação de entrada** em todos os endpoints
- **CORS** configurado para frontend
- **Password policies** do ASP.NET Identity
- **Rate limiting** (configurável)

## 🧪 Testes

### Backend
```bash
# Execute os testes (quando implementados)
dotnet test
```

### Frontend
```bash
# Execute os testes
npm test
```

## 📦 Build e Deploy

### Backend
```bash
# Build para produção
dotnet build --configuration Release

# Publicar
dotnet publish --configuration Release --output ./publish
```

### Frontend
```bash
# Build para produção
npm run build
```

## ✅ Funcionalidades Implementadas

### Sistema Completo
- [x] **Autenticação JWT** com login/logout
- [x] **Recuperação de senha** via email (simulada em desenvolvimento)
- [x] **Dashboard avançado** com gráficos e analytics
- [x] **CRUD completo** para Assuntos a Pesquisar
- [x] **Export para Excel** funcional com ClosedXML
- [x] **Filtros e busca** em todas as telas
- [x] **Design responsivo** com glassmorphism
- [x] **Validação** de formulários
- [x] **Notificações toast**
- [x] **Clean Architecture** implementada
- [x] **CQRS** com MediatR
- [x] **Repository Pattern** com Unit of Work

### Dashboard Analytics
- Cards de resumo com totais
- Gráfico de área com tendência de anomalias
- Gráfico de barras horizontais dos top sites
- Gráfico de pizza com distribuição de assuntos
- Ranking de assuntos com indicadores coloridos
- Filtros por período (data inicial/final)

### Sistema de Autenticação
- Login seguro com JWT
- Tela "Esqueci minha senha" completa
- Reset de senha com token
- Email HTML formatado (simulado)
- Proteção de rotas
- Logout automático em caso de token expirado

### CRUD de Assuntos
- Listagem com paginação
- Busca por nome/descrição
- Filtro por status (ativo/inativo)
- Modal de criação/edição
- Confirmação de exclusão
- Export para Excel (.xlsx)
- Validação de formulários

## 📝 Próximas Implementações

### Funcionalidades Pendentes
- [ ] **CRUD completo** para Categorias, Sites e Anomalias
- [ ] **Sistema de notificações** em tempo real
- [ ] **Logs de auditoria**
- [ ] **Testes unitários e integração**
- [ ] **Docker containers**
- [ ] **CI/CD pipeline**

### Melhorias Técnicas
- [ ] **Cache** com Redis
- [ ] **Logging** estruturado com Serilog
- [ ] **Health checks**
- [ ] **API versioning**
- [ ] **OpenAPI/Swagger** completo
- [ ] **Background jobs** com Hangfire

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

## 🆘 Suporte

Para dúvidas ou problemas:
- Abra uma **issue** no repositório
- Consulte a documentação das tecnologias utilizadas
- Verifique os logs da aplicação

---

**Desenvolvido com ❤️ usando .NET 8 e React 18**