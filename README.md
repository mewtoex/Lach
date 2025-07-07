# 🍔 Sistema de Gerenciamento de Pedidos - Snack Bar

[![CI/CD](https://github.com/your-org/lach-snack-bar/workflows/CI/CD%20Pipeline/badge.svg)](https://github.com/your-org/lach-snack-bar/actions)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue.svg)](https://www.docker.com/)
[![PWA](https://img.shields.io/badge/PWA-Ready-green.svg)](https://web.dev/progressive-web-apps/)

Sistema completo de microserviços para gerenciamento de pedidos de uma snack bar, com backend em C#, PostgreSQL, frontend React com Material-UI, integração WhatsApp e atualizações em tempo real.

## 🏗️ Arquitetura

### Microserviços Backend (C#)
- **AuthService**: Autenticação e autorização
- **ProductService**: Gestão de produtos, categorias e adicionais
- **OrderService**: Processamento de pedidos
- **ProductionQueueService**: Fila de produção
- **DeliveryService**: Gestão de entregas
- **WhatsAppService**: Integração com WhatsApp
- **NotificationService**: Notificações em tempo real
- **RecommendationService**: Recomendações com ML
- **API Gateway**: Roteamento e agregação

### Frontend (React + Material-UI)
- **Portal do Cliente**: Interface para clientes
- **Portal Administrativo**: Gestão completa
- **PWA**: Progressive Web App com funcionalidades offline

### Infraestrutura
- **PostgreSQL**: Banco de dados principal
- **RabbitMQ**: Mensageria entre serviços
- **Redis**: Cache e sessões
- **Docker**: Containerização completa
- **Prometheus + Grafana**: Monitoramento

## 🚀 Funcionalidades Principais

### 📦 Sistema de Produtos e Adicionais
- **Categorias de Produtos**: Organização hierárquica
- **Sistema de Adicionais por Categoria**: 
  - Categorias de adicionais (Frutas, Granola, Adoçantes, Carnes, Queijos, Molhos)
  - Adicionais vinculados a categorias
  - Produtos podem ter múltiplas categorias de adicionais
  - Configurações específicas por produto (máximo de seleções, obrigatoriedade)
  - Preços mínimos e máximos por categoria
  - Ordem de exibição personalizada

### 🛒 Gestão de Pedidos
- Criação e acompanhamento de pedidos
- Status em tempo real
- Histórico completo
- Notificações automáticas

### 🤖 Integração WhatsApp
- Chatbot para pedidos
- Status automático
- Confirmações
- Suporte ao cliente

### 📊 Analytics e ML
- Recomendações personalizadas
- Análise de vendas
- Métricas de performance
- Dashboards interativos

### 📱 PWA (Progressive Web App)
- Funcionalidades offline
- Instalação como app
- Notificações push
- Sincronização automática

## 🏷️ Sistema de Categorias de Adicionais

### Estrutura
```
Produto (Açaí Tradicional)
├── Categoria de Adicional: Frutas
│   ├── Banana (R$ 2,00)
│   ├── Morango (R$ 3,00)
│   ├── Manga (R$ 2,50)
│   └── Kiwi (R$ 3,50)
├── Categoria de Adicional: Granola
│   ├── Granola Tradicional (R$ 1,50)
│   └── Granola de Chocolate (R$ 2,00)
└── Categoria de Adicional: Adoçantes
    ├── Leite Condensado (R$ 1,00)
    └── Mel (R$ 1,50)
```

### Vantagens
- **Flexibilidade**: Produtos podem ter múltiplas categorias
- **Organização**: Adicionais agrupados logicamente
- **Configuração**: Limites e regras por categoria
- **Reutilização**: Categorias podem ser usadas em vários produtos
- **Manutenibilidade**: Fácil adição/remoção de adicionais

### Configurações por Categoria
- **MaxSelections**: Máximo de adicionais selecionáveis
- **IsRequired**: Se é obrigatório selecionar pelo menos um
- **DisplayOrder**: Ordem de exibição
- **MinPrice/MaxPrice**: Faixa de preços
- **Color/Icon**: Personalização visual

## 🛠️ Tecnologias

### Backend
- **.NET 8**: Framework principal
- **Entity Framework Core**: ORM
- **PostgreSQL**: Banco de dados
- **RabbitMQ**: Mensageria
- **Redis**: Cache
- **Serilog**: Logging
- **xUnit**: Testes
- **Prometheus**: Métricas

### Frontend
- **React 18**: Framework
- **Material-UI**: Componentes
- **TypeScript**: Tipagem
- **Redux Toolkit**: Estado
- **React Query**: Cache
- **PWA**: Funcionalidades offline

### DevOps
- **Docker**: Containerização
- **Docker Compose**: Orquestração
- **GitHub Actions**: CI/CD
- **Grafana**: Dashboards
- **AlertManager**: Alertas

## 📦 Instalação e Execução

### Pré-requisitos
- Docker e Docker Compose
- .NET 8 SDK (para desenvolvimento)
- Node.js 18+ (para desenvolvimento)

### Execução Rápida
```bash
# Clone o repositório
git clone <repository-url>
cd snack-bar-system

# Execute com Docker Compose
docker-compose up -d

# Acesse os serviços
# Frontend: http://localhost:3000
# API Gateway: http://localhost:5000
# Grafana: http://localhost:3001
# RabbitMQ Management: http://localhost:15672
```

### Desenvolvimento
```bash
# Backend
cd src/Services
dotnet restore
dotnet build
dotnet test

# Frontend
cd frontend
npm install
npm start

# Testes de carga
./scripts/run-load-tests.ps1

# Monitoramento
./scripts/start-monitoring.ps1
```

## 📚 Documentação

- [API Documentation](docs/api.md)
- [Architecture Guide](docs/architecture.md)
- [Deployment Guide](docs/deployment.md)
- [WhatsApp Integration](docs/whatsapp-integration.md)

## 🔧 Configuração

### Variáveis de Ambiente
```env
# Database
POSTGRES_CONNECTION_STRING=Host=localhost;Database=snackbar;Username=postgres;Password=password

# RabbitMQ
RABBITMQ_HOST=localhost
RABBITMQ_USERNAME=guest
RABBITMQ_PASSWORD=guest

# Redis
REDIS_CONNECTION_STRING=localhost:6379

# WhatsApp
WHATSAPP_SESSION_PATH=/app/sessions
WHATSAPP_WEBHOOK_URL=http://localhost:5000/api/whatsapp/webhook
```

### Configuração de Categorias de Adicionais
```json
{
  "name": "Frutas",
  "description": "Frutas frescas para adicionar",
  "maxSelections": 3,
  "isRequired": false,
  "minPrice": 2.00,
  "maxPrice": 5.00,
  "color": "#FF6B6B",
  "icon": "🍓"
}
```

## 🧪 Testes

### Executar Testes
```bash
# Todos os testes
dotnet test

# Testes específicos
dotnet test --filter "FullyQualifiedName~AuthService"

# Testes de integração
dotnet test --filter "Category=Integration"

# Testes de carga
./scripts/run-load-tests.ps1
```

### Cobertura de Testes
- **Unit Tests**: 90%+
- **Integration Tests**: 85%+
- **Load Tests**: Simulação de 1000 usuários simultâneos

## 📊 Monitoramento

### Métricas Disponíveis
- **Performance**: Response time, throughput
- **Business**: Pedidos por hora, produtos mais vendidos
- **Infrastructure**: CPU, memória, disco
- **Custom**: Métricas específicas do negócio

### Dashboards Grafana
- **Overview**: Visão geral do sistema
- **Orders**: Métricas de pedidos
- **Products**: Performance de produtos
- **Infrastructure**: Recursos do sistema

### Alertas
- **High Error Rate**: Taxa de erro > 5%
- **Slow Response**: Response time > 2s
- **Database Issues**: Conexões lentas
- **Queue Overflow**: Fila de mensagens cheia

## 🔄 CI/CD Pipeline

### GitHub Actions
1. **Build**: Compilação e testes
2. **Security Scan**: Análise de vulnerabilidades
3. **Docker Build**: Criação de imagens
4. **Deploy**: Implantação automática

### Ambientes
- **Development**: Desenvolvimento
- **Staging**: Testes
- **Production**: Produção

## 🚀 Roadmap

### Próximas Funcionalidades
- [ ] **Pagamento Online**: Integração com gateways
- [ ] **Loyalty Program**: Programa de fidelidade
- [ ] **Advanced Analytics**: Análises avançadas
- [ ] **Mobile App**: App nativo
- [ ] **Multi-language**: Suporte a múltiplos idiomas
- [ ] **Advanced ML**: Recomendações mais sofisticadas

### Melhorias Técnicas
- [ ] **GraphQL**: API mais flexível
- [ ] **Event Sourcing**: Auditoria completa
- [ ] **CQRS**: Separação de comandos e consultas
- [ ] **Micro-frontends**: Frontend modular
- [ ] **Service Mesh**: Comunicação entre serviços

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature
3. Commit suas mudanças
4. Push para a branch
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 📞 Suporte

- **Email**: support@snackbar.com
- **Documentação**: [docs/](docs/)
- **Issues**: [GitHub Issues](https://github.com/snackbar/issues)

---

**Desenvolvido com ❤️ para revolucionar a gestão de snack bars!** 