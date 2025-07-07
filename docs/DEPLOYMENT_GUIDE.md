# 🚀 Guia de Deployment - Lach Snack Bar

## 📋 Índice

1. [Pré-requisitos](#pré-requisitos)
2. [Ambientes](#ambientes)
3. [Deployment Local](#deployment-local)
4. [Deployment Staging](#deployment-staging)
5. [Deployment Production](#deployment-production)
6. [CI/CD Pipeline](#cicd-pipeline)
7. [Monitoramento](#monitoramento)
8. [Troubleshooting](#troubleshooting)

## 🔧 Pré-requisitos

### Software Necessário

- **Docker Desktop** 4.0+
- **Docker Compose** 2.0+
- **.NET 8.0 SDK**
- **Node.js** 18+
- **Git**
- **PowerShell** 7+ (Windows) ou **Bash** (Linux/Mac)

### Recursos Mínimos

- **CPU:** 4 cores
- **RAM:** 8GB
- **Storage:** 20GB livre
- **Network:** Conexão estável com internet

### Contas Necessárias

- **GitHub** (para CI/CD)
- **Docker Hub** ou **GitHub Container Registry**
- **PostgreSQL** (opcional, para produção)

## 🌍 Ambientes

### Local (Development)
- **Propósito:** Desenvolvimento e testes
- **URL:** http://localhost:8080
- **Database:** PostgreSQL local
- **Storage:** Local

### Staging
- **Propósito:** Testes de integração
- **URL:** https://staging.lach-snackbar.com
- **Database:** PostgreSQL dedicado
- **Storage:** Cloud storage

### Production
- **Propósito:** Ambiente de produção
- **URL:** https://app.lach-snackbar.com
- **Database:** PostgreSQL cluster
- **Storage:** Cloud storage com backup

## 🏠 Deployment Local

### 1. Clone do Repositório

```bash
git clone https://github.com/your-org/lach-snack-bar.git
cd lach-snack-bar
```

### 2. Configuração Inicial

```bash
# Copiar arquivo de exemplo
cp env.example .env

# Editar variáveis de ambiente
notepad .env  # Windows
# ou
nano .env     # Linux/Mac
```

### 3. Configurações do .env

```bash
# Database
POSTGRES_DB=lach
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres

# RabbitMQ
RABBITMQ_DEFAULT_USER=admin
RABBITMQ_DEFAULT_PASS=admin

# JWT
JWT_SECRET_KEY=your-super-secret-key-here
JWT_ISSUER=lach-snack-bar
JWT_AUDIENCE=lach-customers

# WhatsApp
WHATSAPP_WEBHOOK_URL=http://localhost:8080/api/whatsapp/webhook
WHATSAPP_API_KEY=your-whatsapp-api-key

# Monitoring
PROMETHEUS_METRICS_PORT=9090
GRAFANA_PORT=3000
```

### 4. Executar Deployment

```powershell
# Windows PowerShell
.\scripts\deploy.ps1 -Environment local

# Linux/Mac Bash
./scripts/deploy.sh local
```

### 5. Verificar Deployment

```bash
# Verificar status dos serviços
docker-compose ps

# Verificar logs
docker-compose logs -f

# Testar API
curl http://localhost:8080/health
```

### 6. Acessar Interfaces

- **API Gateway:** http://localhost:8080
- **Grafana:** http://localhost:3000 (admin/admin)
- **Prometheus:** http://localhost:9090
- **RabbitMQ Management:** http://localhost:15672 (admin/admin)
- **PostgreSQL:** localhost:5432

## 🧪 Deployment Staging

### 1. Preparação

```bash
# Configurar credenciais
export GITHUB_USERNAME=your-username
export GITHUB_TOKEN=your-token

# Criar arquivo de configuração staging
cp docker-compose.yml docker-compose.staging.yml
```

### 2. Configuração Staging

Editar `docker-compose.staging.yml`:

```yaml
version: '3.8'

services:
  auth-service:
    image: ghcr.io/your-org/lach/auth-service:staging
    environment:
      - ASPNETCORE_ENVIRONMENT=Staging
      - ConnectionStrings__DefaultConnection=Host=staging-db;Database=lach;Username=postgres;Password=${POSTGRES_PASSWORD}
    # ... outras configurações

  # Repetir para outros serviços
```

### 3. Deployment

```powershell
.\scripts\deploy.ps1 -Environment staging -Tag staging-latest
```

### 4. Verificação

```bash
# Health checks
curl https://staging.lach-snackbar.com/health

# Verificar logs
docker-compose -f docker-compose.staging.yml logs -f
```

## 🚀 Deployment Production

### 1. Preparação

```bash
# Configurar credenciais de produção
export PROD_DB_PASSWORD=your-secure-password
export PROD_RABBITMQ_PASSWORD=your-secure-password
export PROD_JWT_SECRET=your-super-secure-jwt-secret

# Criar arquivo de configuração produção
cp docker-compose.yml docker-compose.production.yml
```

### 2. Configuração Production

Editar `docker-compose.production.yml`:

```yaml
version: '3.8'

services:
  auth-service:
    image: ghcr.io/your-org/lach/auth-service:production
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=prod-db;Database=lach;Username=postgres;Password=${PROD_DB_PASSWORD}
    deploy:
      replicas: 3
      resources:
        limits:
          memory: 512M
          cpus: '0.5'
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  # Configurar load balancer
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
      - ./ssl:/etc/nginx/ssl
    depends_on:
      - api-gateway

  # Configurar backup automático
  backup:
    image: postgres:15
    volumes:
      - ./backups:/backups
    environment:
      - PGPASSWORD=${PROD_DB_PASSWORD}
    command: |
      sh -c '
        while true; do
          pg_dump -h prod-db -U postgres lach > /backups/backup-$(date +%Y%m%d-%H%M%S).sql
          sleep 86400
        done
      '
```

### 3. SSL/TLS Configuration

```bash
# Gerar certificados SSL
mkdir ssl
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout ssl/nginx.key -out ssl/nginx.crt \
  -subj "/C=BR/ST=SP/L=Sao Paulo/O=Lach/CN=app.lach-snackbar.com"
```

### 4. Nginx Configuration

Criar `nginx.conf`:

```nginx
events {
    worker_connections 1024;
}

http {
    upstream api_gateway {
        server api-gateway:5000;
    }

    server {
        listen 80;
        server_name app.lach-snackbar.com;
        return 301 https://$server_name$request_uri;
    }

    server {
        listen 443 ssl;
        server_name app.lach-snackbar.com;

        ssl_certificate /etc/nginx/ssl/nginx.crt;
        ssl_certificate_key /etc/nginx/ssl/nginx.key;

        location / {
            proxy_pass http://api_gateway;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }
    }
}
```

### 5. Deployment

```powershell
.\scripts\deploy.ps1 -Environment production -Tag production-v1.0.0
```

### 6. Verificação Production

```bash
# Health checks
curl https://app.lach-snackbar.com/health

# Verificar métricas
curl https://app.lach-snackbar.com/metrics

# Verificar logs
docker-compose -f docker-compose.production.yml logs -f
```

## 🔄 CI/CD Pipeline

### 1. Configuração GitHub Actions

O pipeline está configurado em `.github/workflows/ci-cd.yml` e inclui:

- **Build e Test:** Compilação e testes automatizados
- **Security Scan:** Análise de vulnerabilidades
- **Docker Build:** Construção de imagens
- **Deployment:** Deploy automático para staging/production

### 2. Triggers

- **Push para `main`:** Deploy para production
- **Push para `develop`:** Deploy para staging
- **Pull Request:** Build e testes

### 3. Secrets Necessários

Configurar no GitHub Repository Settings > Secrets:

```bash
GITHUB_TOKEN=your-github-token
DOCKER_USERNAME=your-docker-username
DOCKER_PASSWORD=your-docker-password
PROD_DB_PASSWORD=your-production-db-password
PROD_RABBITMQ_PASSWORD=your-production-rabbitmq-password
PROD_JWT_SECRET=your-production-jwt-secret
```

### 4. Rollback

```powershell
# Rollback para versão anterior
.\scripts\rollback.ps1 -Environment production -Tag production-v0.9.0

# Rollback para staging
.\scripts\rollback.ps1 -Environment staging -Tag staging-v0.9.0
```

## 📊 Monitoramento

### 1. Métricas Disponíveis

- **Application Metrics:**
  - Request rate
  - Response time
  - Error rate
  - Active connections

- **Business Metrics:**
  - Orders per hour
  - Revenue per day
  - Customer satisfaction
  - WhatsApp message rate

### 2. Alertas Configurados

```yaml
# AlertManager configuration
alerts:
  - name: HighErrorRate
    condition: error_rate > 5%
    duration: 5m
    action: email,slack

  - name: ServiceDown
    condition: service_health == 0
    duration: 1m
    action: pagerduty,slack

  - name: HighLatency
    condition: response_time > 2s
    duration: 10m
    action: slack
```

### 3. Dashboards Grafana

Acessar http://localhost:3000 (admin/admin):

- **System Overview:** Visão geral do sistema
- **Service Metrics:** Métricas por serviço
- **Business Dashboard:** Métricas de negócio
- **Error Tracking:** Rastreamento de erros

### 4. Logs Centralizados

```bash
# Ver logs de todos os serviços
docker-compose logs -f

# Ver logs de um serviço específico
docker-compose logs -f auth-service

# Ver logs com filtro
docker-compose logs -f | grep ERROR
```

## 🔧 Troubleshooting

### Problemas Comuns

#### 1. Serviços não iniciam

```bash
# Verificar logs
docker-compose logs service-name

# Verificar dependências
docker-compose ps

# Reiniciar serviço
docker-compose restart service-name
```

#### 2. Erro de conexão com banco

```bash
# Verificar se PostgreSQL está rodando
docker-compose ps postgres

# Verificar logs do PostgreSQL
docker-compose logs postgres

# Testar conexão
docker-compose exec postgres psql -U postgres -d lach
```

#### 3. Erro de autenticação JWT

```bash
# Verificar variável JWT_SECRET
echo $JWT_SECRET_KEY

# Regenerar secret
export JWT_SECRET_KEY=$(openssl rand -base64 32)
```

#### 4. WhatsApp não conecta

```bash
# Verificar status do WhatsApp Web Service
curl http://localhost:3001/health

# Verificar QR code
curl http://localhost:8080/api/whatsapp/sessions/session-id/qr

# Verificar logs
docker-compose logs whatsapp-web-service
```

#### 5. Performance lenta

```bash
# Verificar uso de recursos
docker stats

# Verificar métricas
curl http://localhost:9090/metrics

# Verificar logs de performance
docker-compose logs | grep "slow"
```

### Comandos Úteis

```bash
# Limpar containers parados
docker container prune

# Limpar imagens não utilizadas
docker image prune

# Limpar volumes não utilizados
docker volume prune

# Limpar tudo
docker system prune -a

# Verificar espaço em disco
df -h

# Verificar uso de memória
free -h

# Verificar processos
top
```

### Contatos de Suporte

- **DevOps:** devops@lach-snackbar.com
- **Infraestrutura:** infra@lach-snackbar.com
- **Emergência:** +55 11 99999-9999

## 📚 Recursos Adicionais

- [Documentação da API](../docs/API_DOCUMENTATION.md)
- [Guia de Desenvolvimento](../docs/DEVELOPMENT_GUIDE.md)
- [Arquitetura do Sistema](../docs/ARCHITECTURE.md)
- [Integração WhatsApp](../WHATSAPP_INTEGRATION.md) 