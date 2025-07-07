# Setup do Sistema Lach

## Pré-requisitos

- Docker Desktop instalado e rodando
- .NET 8 SDK (para desenvolvimento local)
- Node.js 18+ (para desenvolvimento do frontend)
- Git

## Configuração Inicial

### 1. Clone o Repositório
```bash
git clone <repository-url>
cd Lach
```

### 2. Configure as Variáveis de Ambiente
```bash
# Copie o arquivo de exemplo
cp env.example .env

# Edite o arquivo .env com suas configurações
# Especialmente as chaves de API do WhatsApp e Google Maps
```

### 3. Build e Execução

#### Usando o Script PowerShell (Recomendado)
```powershell
# Build e executar
.\build.ps1 -Build -Run

# Apenas executar (se já foi feito build)
.\build.ps1 -Run

# Parar serviços
.\build.ps1 -Stop

# Ver logs
.\build.ps1 -Logs

# Limpar tudo
.\build.ps1 -Clean
```

#### Usando Docker Compose Diretamente
```bash
# Build dos containers
docker-compose build

# Executar em background
docker-compose up -d

# Ver logs
docker-compose logs -f

# Parar serviços
docker-compose down
```

## Verificação da Instalação

### 1. Verificar Containers
```bash
docker-compose ps
```

Todos os containers devem estar com status "Up".

### 2. Testar API Gateway
```bash
curl http://localhost:5000/api/products
```

### 3. Acessar Interfaces

- **API Gateway:** http://localhost:5000
- **Swagger UI:** http://localhost:5000/swagger
- **RabbitMQ Management:** http://localhost:15672
  - Usuário: admin
  - Senha: admin123

## Estrutura dos Microsserviços

### Serviços Implementados
- ✅ **AuthService** - Autenticação e autorização
- ✅ **ProductService** - Gerenciamento de produtos
- ✅ **OrderService** - Gerenciamento de pedidos
- ✅ **API Gateway** - Roteamento e proxy

### Serviços Pendentes
- 🔄 **ProductionQueueService** - Fila de produção
- 🔄 **DeliveryService** - Cálculo de entrega
- 🔄 **WhatsAppService** - Chatbot do WhatsApp
- 🔄 **NotificationService** - Notificações em tempo real
- 🔄 **Frontend** - Interface do cliente
- 🔄 **AdminPortal** - Interface administrativa

## Desenvolvimento Local

### 1. Executar Apenas os Serviços de Infraestrutura
```bash
docker-compose up -d postgres rabbitmq redis
```

### 2. Executar Microsserviços Localmente
```bash
# AuthService
cd src/Services/AuthService
dotnet run

# ProductService
cd src/Services/ProductService
dotnet run

# OrderService
cd src/Services/OrderService
dotnet run

# Gateway
cd src/Gateway
dotnet run
```

### 3. Configuração de Desenvolvimento
Para desenvolvimento local, atualize o arquivo `ocelot.json` no Gateway:

```json
{
  "DownstreamHostAndPorts": [
    {
      "Host": "localhost",
      "Port": 5001  // Porta do serviço local
    }
  ]
}
```

## Troubleshooting

### Problemas Comuns

#### 1. Porta já em uso
```bash
# Verificar processos usando a porta
netstat -ano | findstr :5000

# Parar processo
taskkill /PID <process_id> /F
```

#### 2. Containers não iniciam
```bash
# Verificar logs
docker-compose logs <service-name>

# Rebuild específico
docker-compose build <service-name>
docker-compose up -d <service-name>
```

#### 3. Banco de dados não conecta
```bash
# Verificar se PostgreSQL está rodando
docker-compose logs postgres

# Resetar banco
docker-compose down -v
docker-compose up -d
```

#### 4. RabbitMQ não conecta
```bash
# Verificar logs do RabbitMQ
docker-compose logs rabbitmq

# Acessar interface web
# http://localhost:15672
```

### Logs Úteis

```bash
# Logs de todos os serviços
docker-compose logs -f

# Logs de um serviço específico
docker-compose logs -f auth-service
docker-compose logs -f product-service
docker-compose logs -f order-service
docker-compose logs -f api-gateway

# Logs com timestamp
docker-compose logs -f --timestamps
```

## Próximos Passos

1. **Implementar serviços restantes:**
   - ProductionQueueService
   - DeliveryService
   - WhatsAppService
   - NotificationService

2. **Desenvolver frontend:**
   - Interface do cliente (React + MUI)
   - Portal administrativo

3. **Configurar integrações:**
   - WhatsApp Business API
   - Google Maps API

4. **Implementar testes:**
   - Testes unitários
   - Testes de integração
   - Testes end-to-end

5. **Configurar CI/CD:**
   - GitHub Actions
   - Docker Registry
   - Deploy automatizado

## Suporte

Para dúvidas ou problemas:
1. Verifique os logs dos containers
2. Consulte a documentação da API
3. Abra uma issue no repositório 