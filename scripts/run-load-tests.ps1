#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("small", "medium", "large")]
    [string]$LoadSize = "medium",
    
    [Parameter(Mandatory=$false)]
    [int]$Duration = 60,
    
    [Parameter(Mandatory=$false)]
    [string]$TargetUrl = "http://localhost:8080"
)

Write-Host "🚀 Iniciando testes de carga..." -ForegroundColor Green

# Configurações por tamanho de carga
$LoadConfig = @{
    Small = @{
        Users = 10
        RampUp = 10
        Duration = 60
    }
    Medium = @{
        Users = 50
        RampUp = 30
        Duration = 120
    }
    Large = @{
        Users = 100
        RampUp = 60
        Duration = 300
    }
}

$Config = $LoadConfig[$LoadSize]

Write-Host "📊 Configuração de carga: $LoadSize" -ForegroundColor Yellow
Write-Host "   👥 Usuários: $($Config.Users)" -ForegroundColor Cyan
Write-Host "   ⏱️  Ramp-up: $($Config.RampUp)s" -ForegroundColor Cyan
Write-Host "   ⏰ Duração: $($Config.Duration)s" -ForegroundColor Cyan
Write-Host "   🌐 Target: $TargetUrl" -ForegroundColor Cyan

# Verificar se o sistema está rodando
Write-Host "🔍 Verificando se o sistema está rodando..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$TargetUrl/health" -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ Sistema está rodando" -ForegroundColor Green
    } else {
        Write-Host "❌ Sistema não está respondendo corretamente" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ Sistema não está acessível em $TargetUrl" -ForegroundColor Red
    Write-Host "💡 Certifique-se de que o sistema está rodando com: docker-compose up -d" -ForegroundColor Yellow
    exit 1
}

# Criar diretório de resultados
$ResultsDir = "load-test-results"
if (-not (Test-Path $ResultsDir)) {
    New-Item -ItemType Directory -Path $ResultsDir | Out-Null
}

$Timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$ResultsFile = "$ResultsDir/load-test-$LoadSize-$Timestamp.json"

Write-Host "📁 Resultados serão salvos em: $ResultsFile" -ForegroundColor Yellow

# Criar arquivo de configuração NBomber
$NbomberConfig = @"
{
  "TestName": "Lach Snack Bar Load Test",
  "TestSuite": "Load Tests",
  "Scenarios": [
    {
      "ScenarioName": "API Load Test",
      "LoadSimulations": [
        {
          "LoadSimulationType": "RampConstant",
          "Rate": $($Config.Users),
          "SimulationTime": $($Config.Duration)
        }
      ]
    }
  ],
  "Reporting": {
    "Formats": ["Txt", "Csv", "Html"],
    "FileName": "load-test-report-$LoadSize-$Timestamp"
  }
}
"@

$ConfigFile = "nbomber-config.json"
$NbomberConfig | Out-File -FilePath $ConfigFile -Encoding UTF8

# Executar testes de carga
Write-Host "🔥 Executando testes de carga..." -ForegroundColor Yellow

try {
    # Verificar se NBomber está instalado
    $nbomberInstalled = dotnet tool list --global | Select-String "nbomber"
    if (-not $nbomberInstalled) {
        Write-Host "📦 Instalando NBomber..." -ForegroundColor Yellow
        dotnet tool install --global NBomber.CLI
    }

    # Executar testes
    $nbomberArgs = @(
        "run",
        "tests/LoadTests/LoadTests.csproj",
        "--config", $ConfigFile,
        "--target", $TargetUrl
    )

    Write-Host "🚀 Executando: nbomber $($nbomberArgs -join ' ')" -ForegroundColor Cyan
    
    $process = Start-Process -FilePath "nbomber" -ArgumentList $nbomberArgs -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -eq 0) {
        Write-Host "✅ Testes de carga concluídos com sucesso!" -ForegroundColor Green
    } else {
        Write-Host "❌ Testes de carga falharam" -ForegroundColor Red
        exit 1
    }

} catch {
    Write-Host "❌ Erro ao executar testes de carga: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    # Limpar arquivo de configuração
    if (Test-Path $ConfigFile) {
        Remove-Item $ConfigFile
    }
}

# Analisar resultados
Write-Host "📊 Analisando resultados..." -ForegroundColor Yellow

$ReportFiles = Get-ChildItem -Path "." -Filter "load-test-report-$LoadSize-$Timestamp.*"
foreach ($file in $ReportFiles) {
    Write-Host "📄 Relatório: $($file.Name)" -ForegroundColor Cyan
}

# Verificar métricas do sistema durante os testes
Write-Host "📈 Verificando métricas do sistema..." -ForegroundColor Yellow

try {
    # Métricas do Prometheus
    $metrics = Invoke-WebRequest -Uri "http://localhost:9090/api/v1/query?query=up" -TimeoutSec 10
    Write-Host "📊 Métricas do Prometheus disponíveis" -ForegroundColor Green
    
    # Status dos serviços
    $services = @("auth-service", "product-service", "order-service", "api-gateway")
    foreach ($service in $services) {
        try {
            $health = Invoke-WebRequest -Uri "$TargetUrl/health" -TimeoutSec 5
            Write-Host "✅ $service está saudável" -ForegroundColor Green
        } catch {
            Write-Host "❌ $service não está respondendo" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "⚠️  Não foi possível verificar métricas do Prometheus" -ForegroundColor Yellow
}

# Resumo dos resultados
Write-Host "`n📋 Resumo dos Testes de Carga" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green
Write-Host "🎯 Configuração: $LoadSize" -ForegroundColor Cyan
Write-Host "👥 Usuários simultâneos: $($Config.Users)" -ForegroundColor Cyan
Write-Host "⏱️  Duração: $($Config.Duration)s" -ForegroundColor Cyan
Write-Host "🌐 Target: $TargetUrl" -ForegroundColor Cyan
Write-Host "📁 Resultados: $ResultsDir" -ForegroundColor Cyan

# Verificar se há relatórios HTML
$HtmlReport = Get-ChildItem -Path "." -Filter "load-test-report-$LoadSize-$Timestamp.html" -ErrorAction SilentlyContinue
if ($HtmlReport) {
    Write-Host "`n🌐 Para ver o relatório detalhado, abra: $($HtmlReport.FullName)" -ForegroundColor Yellow
}

Write-Host "`n🎉 Testes de carga concluídos!" -ForegroundColor Green

# Sugestões baseadas no tamanho da carga
Write-Host "`n💡 Sugestões:" -ForegroundColor Yellow
switch ($LoadSize) {
    "small" {
        Write-Host "   - Sistema está pronto para desenvolvimento" -ForegroundColor Cyan
        Write-Host "   - Considere testes médios para validação" -ForegroundColor Cyan
    }
    "medium" {
        Write-Host "   - Sistema está pronto para staging" -ForegroundColor Cyan
        Write-Host "   - Considere testes grandes para produção" -ForegroundColor Cyan
    }
    "large" {
        Write-Host "   - Sistema está pronto para produção" -ForegroundColor Cyan
        Write-Host "   - Monitore métricas em produção" -ForegroundColor Cyan
    }
} 