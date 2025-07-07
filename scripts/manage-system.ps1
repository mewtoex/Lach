# Lach System - Management Script
# Script para gerenciar o sistema completo de microsserviços

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("start", "stop", "restart", "status", "logs", "clean", "test", "monitor")]
    [string]$Action,
    
    [string]$Service = ""
)

Write-Host "🏪 Lach System - Management Console" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan

function Show-Status {
    Write-Host "`n📊 Status dos Serviços:" -ForegroundColor Cyan
    Write-Host "=====================" -ForegroundColor Cyan
    
    $services = @(
        @{Name="API Gateway"; Port=5000; Container="lach-api-gateway"},
        @{Name="Auth Service"; Port=5001; Container="lach-auth-service"},
        @{Name="Product Service"; Port=5002; Container="lach-product-service"},
        @{Name="Order Service"; Port=5003; Container="lach-order-service"},
        @{Name="Production Queue Service"; Port=5004; Container="lach-production-queue-service"},
        @{Name="Delivery Service"; Port=5005; Container="lach-delivery-service"},
        @{Name="WhatsApp Service"; Port=5006; Container="lach-whatsapp-service"},
        @{Name="Notification Service"; Port=5007; Container="lach-notification-service"},
        @{Name="Prometheus"; Port=9090; Container="lach-prometheus"},
        @{Name="Grafana"; Port=3002; Container="lach-grafana"},
        @{Name="Seq"; Port=5341; Container="lach-seq"},
        @{Name="AlertManager"; Port=9093; Container="lach-alertmanager"},
        @{Name="Frontend"; Port=3000; Container="lach-frontend"},
        @{Name="Admin Portal"; Port=3001; Container="lach-admin-portal"}
    )
    
    foreach ($service in $services) {
        $container = docker ps --filter "name=$($service.Container)" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" 2>$null
        if ($container -and $container.Length -gt 1) {
            $status = "✅ Running"
            $color = "Green"
        } else {
            $status = "❌ Stopped"
            $color = "Red"
        }
        
        Write-Host "$($service.Name.PadRight(25)) | $status | http://localhost:$($service.Port)" -ForegroundColor $color
    }
    
    Write-Host "`n🔗 URLs de Acesso:" -ForegroundColor Yellow
    Write-Host "API Gateway: http://localhost:5000" -ForegroundColor White
    Write-Host "Frontend: http://localhost:3000" -ForegroundColor White
    Write-Host "Admin Portal: http://localhost:3001" -ForegroundColor White
    Write-Host "Grafana: http://localhost:3002 (admin/admin123)" -ForegroundColor White
    Write-Host "Prometheus: http://localhost:9090" -ForegroundColor White
    Write-Host "Seq Logs: http://localhost:5341" -ForegroundColor White
    Write-Host "AlertManager: http://localhost:9093" -ForegroundColor White
}

function Start-System {
    Write-Host "`n🚀 Iniciando o sistema..." -ForegroundColor Green
    
    # Check if .env exists
    if (-not (Test-Path ".env")) {
        Write-Host "⚠️  Arquivo .env não encontrado. Copiando de .env.example..." -ForegroundColor Yellow
        if (Test-Path ".env.example") {
            Copy-Item ".env.example" ".env"
            Write-Host "✅ Arquivo .env criado. Configure as variáveis de ambiente antes de continuar." -ForegroundColor Green
            return
        } else {
            Write-Host "❌ Arquivo .env.example não encontrado!" -ForegroundColor Red
            return
        }
    }
    
    # Start infrastructure services first
    Write-Host "📦 Iniciando serviços de infraestrutura..." -ForegroundColor Blue
    docker-compose up -d postgres rabbitmq redis
    
    # Wait for infrastructure to be ready
    Write-Host "⏳ Aguardando serviços de infraestrutura..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10
    
    # Start all services
    Write-Host "🚀 Iniciando todos os serviços..." -ForegroundColor Blue
    docker-compose up -d
    
    Write-Host "✅ Sistema iniciado com sucesso!" -ForegroundColor Green
    Show-Status
}

function Stop-System {
    Write-Host "`n🛑 Parando o sistema..." -ForegroundColor Red
    docker-compose down
    Write-Host "✅ Sistema parado!" -ForegroundColor Green
}

function Restart-System {
    Write-Host "`n🔄 Reiniciando o sistema..." -ForegroundColor Yellow
    Stop-System
    Start-Sleep -Seconds 5
    Start-System
}

function Show-Logs {
    if ($Service -eq "") {
        Write-Host "`n📝 Logs de todos os serviços (Ctrl+C para sair):" -ForegroundColor Cyan
        docker-compose logs -f
    } else {
        Write-Host "`n📝 Logs do serviço $Service (Ctrl+C para sair):" -ForegroundColor Cyan
        docker-compose logs -f $Service
    }
}

function Clean-System {
    Write-Host "`n🧹 Limpando o sistema..." -ForegroundColor Yellow
    
    Write-Host "🛑 Parando containers..." -ForegroundColor Blue
    docker-compose down
    
    Write-Host "🗑️  Removendo containers..." -ForegroundColor Blue
    docker-compose rm -f
    
    Write-Host "🗑️  Removendo volumes..." -ForegroundColor Blue
    docker volume prune -f
    
    Write-Host "🗑️  Removendo imagens..." -ForegroundColor Blue
    docker image prune -f
    
    Write-Host "✅ Sistema limpo!" -ForegroundColor Green
}

function Run-Tests {
    Write-Host "`n🧪 Executando testes..." -ForegroundColor Cyan
    
    if (Test-Path "scripts/run-tests.ps1") {
        & "scripts/run-tests.ps1"
    } else {
        Write-Host "❌ Script de testes não encontrado!" -ForegroundColor Red
    }
}

function Show-Monitoring {
    Write-Host "`n📊 Monitoramento do Sistema" -ForegroundColor Cyan
    Write-Host "=========================" -ForegroundColor Cyan
    
    Write-Host "`n🔗 Dashboards:" -ForegroundColor Yellow
    Write-Host "Grafana: http://localhost:3002 (admin/admin123)" -ForegroundColor White
    Write-Host "Prometheus: http://localhost:9090" -ForegroundColor White
    Write-Host "Seq Logs: http://localhost:5341" -ForegroundColor White
    Write-Host "AlertManager: http://localhost:9093" -ForegroundColor White
    
    Write-Host "`n🏥 Health Checks:" -ForegroundColor Yellow
    $healthEndpoints = @(
        @{Name="API Gateway"; Url="http://localhost:5000/health"},
        @{Name="Auth Service"; Url="http://localhost:5001/health"},
        @{Name="Product Service"; Url="http://localhost:5002/health"},
        @{Name="Order Service"; Url="http://localhost:5003/health"},
        @{Name="Production Queue Service"; Url="http://localhost:5004/health"},
        @{Name="Delivery Service"; Url="http://localhost:5005/health"},
        @{Name="WhatsApp Service"; Url="http://localhost:5006/health"},
        @{Name="Notification Service"; Url="http://localhost:5007/health"}
    )
    
    foreach ($endpoint in $healthEndpoints) {
        try {
            $response = Invoke-WebRequest -Uri $endpoint.Url -TimeoutSec 5 -ErrorAction Stop
            if ($response.StatusCode -eq 200) {
                Write-Host "$($endpoint.Name): ✅ Healthy" -ForegroundColor Green
            } else {
                Write-Host "$($endpoint.Name): ⚠️  Unhealthy ($($response.StatusCode))" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "$($endpoint.Name): ❌ Unreachable" -ForegroundColor Red
        }
    }
    
    Write-Host "`n📈 Métricas Rápidas:" -ForegroundColor Yellow
    try {
        $metrics = Invoke-WebRequest -Uri "http://localhost:9090/api/v1/query?query=up" -TimeoutSec 5
        $data = $metrics.Content | ConvertFrom-Json
        if ($data.data.result) {
            Write-Host "Prometheus: ✅ Coletando métricas" -ForegroundColor Green
        } else {
            Write-Host "Prometheus: ⚠️  Sem dados" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "Prometheus: ❌ Inacessível" -ForegroundColor Red
    }
}

# Main execution
switch ($Action.ToLower()) {
    "start" { Start-System }
    "stop" { Stop-System }
    "restart" { Restart-System }
    "status" { Show-Status }
    "logs" { Show-Logs }
    "clean" { Clean-System }
    "test" { Run-Tests }
    "monitor" { Show-Monitoring }
    default {
        Write-Host "❌ Ação inválida!" -ForegroundColor Red
        Write-Host "Ações disponíveis: start, stop, restart, status, logs, clean, test, monitor" -ForegroundColor Yellow
    }
} 