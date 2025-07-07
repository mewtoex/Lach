# 🧪 Testes e 📊 Monitoramento - Lach System

## 🧪 Estratégia de Testes

### Tipos de Testes Implementados

#### 1. Testes Unitários
- **Framework**: xUnit
- **Assertions**: FluentAssertions
- **Mocking**: Moq
- **Cobertura**: Coverlet

**Serviços com Testes Unitários:**
- ✅ AuthService
- ✅ OrderService
- ✅ ProductionQueueService

**Estrutura dos Testes:**
```
src/Tests/
├── AuthService.Tests/
│   ├── Services/
│   │   └── AuthServiceTests.cs
│   └── Controllers/
│       └── AuthControllerTests.cs
├── OrderService.Tests/
│   └── Services/
│       └── OrderServiceTests.cs
└── ProductionQueueService.Tests/
    └── Services/
        └── ProductionQueueServiceTests.cs
```

#### 2. Testes de Integração
- **Database**: Entity Framework In-Memory
- **HTTP**: Microsoft.AspNetCore.Mvc.Testing
- **API Testing**: TestServer

#### 3. Testes de Carga
- **Framework**: NBomber
- **Scenarios**: Auth, Product, Order, Full System
- **Métricas**: RPS, Latency, Error Rate

### Executando os Testes

#### Testes Unitários
```bash
# Todos os testes
./scripts/run-tests.ps1

# Por tipo
./scripts/run-tests.ps1 -Unit
./scripts/run-tests.ps1 -Integration

# Com cobertura
./scripts/run-tests.ps1 -Coverage

# Serviço específico
./scripts/run-tests.ps1 -Service AuthService
```

#### Testes de Carga
```bash
# Cenários disponíveis
./scripts/load-test.ps1 -Scenario auth      # Autenticação
./scripts/load-test.ps1 -Scenario product   # Produtos
./scripts/load-test.ps1 -Scenario order     # Pedidos
./scripts/load-test.ps1 -Scenario full      # Sistema completo

# Configurações
./scripts/load-test.ps1 -Duration 300 -Rate 500  # 5min, 500 RPS
```

### Exemplos de Testes

#### Teste Unitário - AuthService
```csharp
[Fact]
public async Task RegisterAsync_WithValidData_ShouldCreateUser()
{
    // Arrange
    var request = new RegisterRequest
    {
        Name = "Test User",
        Email = "test@example.com",
        Password = "password123",
        Role = UserRole.Customer
    };

    // Act
    var result = await _authService.RegisterAsync(request);

    // Assert
    result.Should().NotBeNull();
    result.Email.Should().Be(request.Email);
    result.Role.Should().Be(request.Role);
}
```

#### Teste de Carga - NBomber
```csharp
var scenario = Scenario.Create("auth_load_test", async context =>
{
    var request = Http.CreateRequest("POST", $"{_baseUrl}/api/auth/register")
        .WithHeader("Content-Type", "application/json")
        .WithBody(JsonContent.Create(registerRequest));

    var response = await Http.Send(httpClient, request);
    return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
})
.WithLoadSimulations(
    Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
);
```

## 📊 Sistema de Monitoramento

### Stack de Monitoramento

#### 1. Prometheus
- **Porta**: 9090
- **Função**: Coleta de métricas
- **Retenção**: 200 horas
- **Alertas**: Configurados

#### 2. Grafana
- **Porta**: 3002
- **Credenciais**: admin/admin123
- **Dashboards**: Sistema principal
- **Datasource**: Prometheus

#### 3. Seq
- **Porta**: 5341
- **Função**: Logs estruturados
- **Busca**: Avançada
- **Retenção**: Configurável

#### 4. AlertManager
- **Porta**: 9093
- **Função**: Gerenciamento de alertas
- **Integração**: Slack (configurável)

### Métricas Coletadas

#### HTTP Metrics
```prometheus
# Request Rate
rate(http_requests_total[5m])

# Response Time (95th percentile)
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))

# Error Rate
rate(errors_total[5m])
```

#### Business Metrics
```prometheus
# Orders Created
rate(orders_created_total[5m])

# Order Status Changes
rate(order_status_changes_total[5m])

# Processing Duration
histogram_quantile(0.95, rate(order_processing_duration_seconds_bucket[5m]))
```

#### System Metrics
```prometheus
# Memory Usage
process_resident_memory_bytes / 1024 / 1024

# CPU Usage
rate(process_cpu_seconds_total[5m]) * 100

# Database Connections
active_connections
```

### Alertas Configurados

#### Critical Alerts
```yaml
- alert: ServiceDown
  expr: up == 0
  for: 1m
  labels:
    severity: critical
  annotations:
    summary: "Service {{ $labels.job }} is down"
```

#### Warning Alerts
```yaml
- alert: HighErrorRate
  expr: rate(errors_total[5m]) > 0.1
  for: 2m
  labels:
    severity: warning
  annotations:
    summary: "High error rate detected"
```

### Dashboards do Grafana

#### Dashboard Principal
- **HTTP Request Rate**: Taxa de requisições por endpoint
- **Response Time**: Tempo de resposta (95th percentile)
- **Error Rate**: Taxa de erros por serviço
- **Business Metrics**: Pedidos, status changes, processing time
- **System Health**: CPU, memória, conexões

#### Painéis Específicos
1. **Performance**: Latência, throughput, error rates
2. **Business**: Pedidos, fila de produção, entregas
3. **Infrastructure**: Recursos do sistema, saúde dos serviços

### Logs Estruturados

#### Configuração Serilog
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Lach-System")
    .WriteTo.Console()
    .WriteTo.File("logs/lach-.log", rollingInterval: RollingInterval.Day)
    .WriteTo.Seq("http://seq:5341")
    .CreateLogger();
```

#### Exemplos de Logs
```json
{
  "Timestamp": "2024-01-15T10:30:00.000Z",
  "Level": "Information",
  "Message": "Order created successfully",
  "Properties": {
    "OrderId": "123e4567-e89b-12d3-a456-426614174000",
    "CustomerId": "456e7890-e89b-12d3-a456-426614174001",
    "TotalAmount": 31.80,
    "Service": "OrderService"
  }
}
```

### Health Checks

#### Endpoints de Saúde
```bash
# API Gateway
curl http://localhost:5000/health

# Auth Service
curl http://localhost:5001/health

# Product Service
curl http://localhost:5002/health

# Order Service
curl http://localhost:5003/health
```

#### Resposta de Health Check
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "Database",
      "status": "Healthy",
      "duration": "00:00:00.1234567"
    },
    {
      "name": "RabbitMQ",
      "status": "Healthy",
      "duration": "00:00:00.0456789"
    }
  ],
  "totalDuration": "00:00:00.2345678"
}
```

## 🔧 Configuração Avançada

### Prometheus Configuration
```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'api-gateway'
    static_configs:
      - targets: ['api-gateway:5000']
    metrics_path: '/metrics'
    scrape_interval: 10s
```

### Grafana Datasource
```yaml
apiVersion: 1
datasources:
  - name: Prometheus
    type: prometheus
    access: proxy
    url: http://prometheus:9090
    isDefault: true
```

### AlertManager Configuration
```yaml
global:
  resolve_timeout: 5m
  slack_api_url: 'https://hooks.slack.com/services/YOUR_WEBHOOK'

route:
  group_by: ['alertname']
  group_wait: 10s
  group_interval: 10s
  repeat_interval: 1h
  receiver: 'slack.critical'
```

## 📈 Métricas de Negócio

### KPIs Monitorados
1. **Order Processing Time**: Tempo médio de processamento de pedidos
2. **Queue Backlog**: Pedidos na fila de produção
3. **Error Rate**: Taxa de erros por serviço
4. **Response Time**: Tempo de resposta das APIs
5. **Throughput**: Pedidos processados por hora

### Alertas de Negócio
- **High Order Processing Time**: > 5 minutos
- **Queue Backlog**: > 10 pedidos
- **High Error Rate**: > 5% de erros
- **Service Unavailable**: Serviço down por > 1 minuto

## 🚀 Próximos Passos

### Testes
- [ ] Testes E2E com Playwright
- [ ] Testes de Performance com K6
- [ ] Testes de Segurança
- [ ] Testes de API com Postman Collections

### Monitoramento
- [ ] APM com Jaeger
- [ ] Métricas customizadas de negócio
- [ ] Alertas via WhatsApp/Email
- [ ] Dashboard mobile
- [ ] Análise preditiva de falhas 