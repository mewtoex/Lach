#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("start", "stop", "status", "logs", "metrics", "alerts", "dashboard")]
    [string]$Action,
    
    [Parameter(Mandatory=$false)]
    [string]$Service = "all"
)

Write-Host "📊 Sistema de Monitoramento Lach Snack Bar" -ForegroundColor Green

# Configurações
$MonitoringServices = @{
    Prometheus = @{
        Port = 9090
        Container = "lach-prometheus"
        Config = "monitoring/prometheus.yml"
    }
    Grafana = @{
        Port = 3000
        Container = "lach-grafana"
        Config = "monitoring/grafana/dashboards"
    }
    AlertManager = @{
        Port = 9093
        Container = "lach-alertmanager"
        Config = "monitoring/alertmanager.yml"
    }
}

function Start-Monitoring {
    Write-Host "🚀 Iniciando sistema de monitoramento..." -ForegroundColor Yellow
    
    # Verificar se Docker está rodando
    try {
        docker version | Out-Null
    } catch {
        Write-Host "❌ Docker não está rodando. Inicie o Docker Desktop primeiro." -ForegroundColor Red
        exit 1
    }
    
    # Criar diretórios de configuração
    $dirs = @("monitoring", "monitoring/grafana", "monitoring/grafana/dashboards", "monitoring/grafana/provisioning")
    foreach ($dir in $dirs) {
        if (-not (Test-Path $dir)) {
            New-Item -ItemType Directory -Path $dir | Out-Null
            Write-Host "📁 Criado diretório: $dir" -ForegroundColor Cyan
        }
    }
    
    # Configurar Prometheus
    if (-not (Test-Path "monitoring/prometheus.yml")) {
        $prometheusConfig = @"
global:
  scrape_interval: 15s
  evaluation_interval: 15s

rule_files:
  - "alert_rules.yml"

alerting:
  alertmanagers:
    - static_configs:
        - targets:
          - alertmanager:9093

scrape_configs:
  - job_name: 'lach-services'
    static_configs:
      - targets: ['auth-service:5001', 'product-service:5002', 'order-service:5003', 'api-gateway:8080']
    metrics_path: '/metrics'
    scrape_interval: 10s

  - job_name: 'rabbitmq'
    static_configs:
      - targets: ['rabbitmq:15692']
    scrape_interval: 30s

  - job_name: 'postgres'
    static_configs:
      - targets: ['postgres:5432']
    scrape_interval: 30s
"@
        $prometheusConfig | Out-File -FilePath "monitoring/prometheus.yml" -Encoding UTF8
        Write-Host "📝 Configuração do Prometheus criada" -ForegroundColor Green
    }
    
    # Configurar AlertManager
    if (-not (Test-Path "monitoring/alertmanager.yml")) {
        $alertManagerConfig = @"
global:
  resolve_timeout: 5m

route:
  group_by: ['alertname']
  group_wait: 10s
  group_interval: 10s
  repeat_interval: 1h
  receiver: 'web.hook'

receivers:
  - name: 'web.hook'
    webhook_configs:
      - url: 'http://localhost:5007/api/notifications/alerts'
        send_resolved: true

inhibit_rules:
  - source_match:
      severity: 'critical'
    target_match:
      severity: 'warning'
    equal: ['alertname', 'dev', 'instance']
"@
        $alertManagerConfig | Out-File -FilePath "monitoring/alertmanager.yml" -Encoding UTF8
        Write-Host "📝 Configuração do AlertManager criada" -ForegroundColor Green
    }
    
    # Configurar Grafana
    if (-not (Test-Path "monitoring/grafana/provisioning/datasources")) {
        New-Item -ItemType Directory -Path "monitoring/grafana/provisioning/datasources" | Out-Null
    }
    
    if (-not (Test-Path "monitoring/grafana/provisioning/datasources/prometheus.yml")) {
        $grafanaDatasource = @"
apiVersion: 1

datasources:
  - name: Prometheus
    type: prometheus
    access: proxy
    url: http://prometheus:9090
    isDefault: true
"@
        $grafanaDatasource | Out-File -FilePath "monitoring/grafana/provisioning/datasources/prometheus.yml" -Encoding UTF8
        Write-Host "📝 Datasource do Grafana criada" -ForegroundColor Green
    }
    
    # Iniciar serviços de monitoramento
    Write-Host "🐳 Iniciando containers de monitoramento..." -ForegroundColor Yellow
    
    docker-compose -f docker-compose.monitoring.yml up -d
    
    # Aguardar serviços iniciarem
    Write-Host "⏳ Aguardando serviços iniciarem..." -ForegroundColor Yellow
    Start-Sleep -Seconds 30
    
    # Verificar status
    Show-MonitoringStatus
    
    Write-Host "✅ Sistema de monitoramento iniciado!" -ForegroundColor Green
    Write-Host "🌐 Acesse:" -ForegroundColor Cyan
    Write-Host "   Grafana: http://localhost:3000 (admin/admin)" -ForegroundColor Cyan
    Write-Host "   Prometheus: http://localhost:9090" -ForegroundColor Cyan
    Write-Host "   AlertManager: http://localhost:9093" -ForegroundColor Cyan
}

function Stop-Monitoring {
    Write-Host "🛑 Parando sistema de monitoramento..." -ForegroundColor Yellow
    
    docker-compose -f docker-compose.monitoring.yml down
    
    Write-Host "✅ Sistema de monitoramento parado!" -ForegroundColor Green
}

function Show-MonitoringStatus {
    Write-Host "📊 Status do Sistema de Monitoramento" -ForegroundColor Yellow
    Write-Host "=====================================" -ForegroundColor Yellow
    
    $services = @("prometheus", "grafana", "alertmanager")
    
    foreach ($service in $services) {
        try {
            $container = docker ps --filter "name=$service" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
            if ($container -and $container -notlike "*NAMES*") {
                Write-Host "✅ $service está rodando" -ForegroundColor Green
            } else {
                Write-Host "❌ $service não está rodando" -ForegroundColor Red
            }
        } catch {
            Write-Host "❌ $service - Erro ao verificar status" -ForegroundColor Red
        }
    }
    
    # Verificar endpoints
    Write-Host "`n🌐 Verificando endpoints..." -ForegroundColor Yellow
    
    $endpoints = @{
        "Grafana" = "http://localhost:3000"
        "Prometheus" = "http://localhost:9090"
        "AlertManager" = "http://localhost:9093"
    }
    
    foreach ($name in $endpoints.Keys) {
        $url = $endpoints[$name]
        try {
            $response = Invoke-WebRequest -Uri $url -TimeoutSec 5 -ErrorAction Stop
            if ($response.StatusCode -eq 200) {
                Write-Host "✅ $name está acessível" -ForegroundColor Green
            } else {
                Write-Host "⚠️  $name retornou status $($response.StatusCode)" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "❌ $name não está acessível" -ForegroundColor Red
        }
    }
}

function Show-MonitoringLogs {
    param([string]$Service = "all")
    
    Write-Host "📋 Logs do Sistema de Monitoramento" -ForegroundColor Yellow
    Write-Host "===================================" -ForegroundColor Yellow
    
    if ($Service -eq "all") {
        docker-compose -f docker-compose.monitoring.yml logs -f
    } else {
        docker-compose -f docker-compose.monitoring.yml logs -f $Service
    }
}

function Show-MonitoringMetrics {
    Write-Host "📈 Métricas do Sistema" -ForegroundColor Yellow
    Write-Host "=====================" -ForegroundColor Yellow
    
    try {
        # Métricas básicas do Prometheus
        $metrics = @(
            "up",
            "http_requests_total",
            "http_request_duration_seconds",
            "process_cpu_seconds_total",
            "process_resident_memory_bytes"
        )
        
        foreach ($metric in $metrics) {
            Write-Host "`n📊 $metric" -ForegroundColor Cyan
            try {
                $response = Invoke-WebRequest -Uri "http://localhost:9090/api/v1/query?query=$metric" -TimeoutSec 5
                $data = $response.Content | ConvertFrom-Json
                
                if ($data.data.result) {
                    foreach ($result in $data.data.result) {
                        $instance = $result.metric.instance
                        $value = $result.value[1]
                        Write-Host "   $instance : $value" -ForegroundColor White
                    }
                } else {
                    Write-Host "   Nenhum dado disponível" -ForegroundColor Gray
                }
            } catch {
                Write-Host "   Erro ao buscar métrica" -ForegroundColor Red
            }
        }
        
        # Métricas de negócio
        Write-Host "`n💼 Métricas de Negócio" -ForegroundColor Cyan
        $businessMetrics = @(
            "orders_created_total",
            "orders_completed_total",
            "revenue_total",
            "whatsapp_messages_sent_total"
        )
        
        foreach ($metric in $businessMetrics) {
            Write-Host "📊 $metric" -ForegroundColor Cyan
            try {
                $response = Invoke-WebRequest -Uri "http://localhost:9090/api/v1/query?query=$metric" -TimeoutSec 5
                $data = $response.Content | ConvertFrom-Json
                
                if ($data.data.result) {
                    foreach ($result in $data.data.result) {
                        $value = $result.value[1]
                        Write-Host "   $value" -ForegroundColor White
                    }
                } else {
                    Write-Host "   Nenhum dado disponível" -ForegroundColor Gray
                }
            } catch {
                Write-Host "   Erro ao buscar métrica" -ForegroundColor Red
            }
        }
        
    } catch {
        Write-Host "❌ Erro ao acessar métricas do Prometheus" -ForegroundColor Red
        Write-Host "💡 Certifique-se de que o Prometheus está rodando" -ForegroundColor Yellow
    }
}

function Show-MonitoringAlerts {
    Write-Host "🚨 Alertas Ativos" -ForegroundColor Yellow
    Write-Host "================" -ForegroundColor Yellow
    
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:9093/api/v1/alerts" -TimeoutSec 5
        $alerts = $response.Content | ConvertFrom-Json
        
        if ($alerts) {
            foreach ($alert in $alerts) {
                $status = $alert.status.state
                $name = $alert.labels.alertname
                $severity = $alert.labels.severity
                
                switch ($status) {
                    "active" { 
                        Write-Host "🔴 $name ($severity)" -ForegroundColor Red
                        Write-Host "   $($alert.annotations.description)" -ForegroundColor Gray
                    }
                    "suppressed" { 
                        Write-Host "🟡 $name ($severity)" -ForegroundColor Yellow
                    }
                    default { 
                        Write-Host "⚪ $name ($severity)" -ForegroundColor Gray
                    }
                }
            }
        } else {
            Write-Host "✅ Nenhum alerta ativo" -ForegroundColor Green
        }
    } catch {
        Write-Host "❌ Erro ao buscar alertas" -ForegroundColor Red
        Write-Host "💡 Certifique-se de que o AlertManager está rodando" -ForegroundColor Yellow
    }
}

function Open-MonitoringDashboard {
    Write-Host "🌐 Abrindo dashboards..." -ForegroundColor Yellow
    
    $dashboards = @{
        "Grafana" = "http://localhost:3000"
        "Prometheus" = "http://localhost:9090"
        "AlertManager" = "http://localhost:9093"
    }
    
    foreach ($name in $dashboards.Keys) {
        $url = $dashboards[$name]
        Write-Host "📊 Abrindo $name..." -ForegroundColor Cyan
        Start-Process $url
        Start-Sleep -Seconds 2
    }
    
    Write-Host "✅ Dashboards abertos!" -ForegroundColor Green
}

# Executar ação solicitada
switch ($Action) {
    "start" { Start-Monitoring }
    "stop" { Stop-Monitoring }
    "status" { Show-MonitoringStatus }
    "logs" { Show-MonitoringLogs -Service $Service }
    "metrics" { Show-MonitoringMetrics }
    "alerts" { Show-MonitoringAlerts }
    "dashboard" { Open-MonitoringDashboard }
    default {
        Write-Host "❌ Ação '$Action' não reconhecida" -ForegroundColor Red
        Write-Host "💡 Ações disponíveis: start, stop, status, logs, metrics, alerts, dashboard" -ForegroundColor Yellow
    }
} 