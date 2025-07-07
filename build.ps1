# Script para build e execução do Sistema Lach
param(
    [switch]$Build,
    [switch]$Run,
    [switch]$Stop,
    [switch]$Clean,
    [switch]$Logs
)

Write-Host "=== Sistema de Gerenciamento de Pedidos Lach ===" -ForegroundColor Green

if ($Build) {
    Write-Host "Construindo containers..." -ForegroundColor Yellow
    docker-compose build
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Build concluído com sucesso!" -ForegroundColor Green
    } else {
        Write-Host "Erro no build!" -ForegroundColor Red
        exit 1
    }
}

if ($Run) {
    Write-Host "Iniciando serviços..." -ForegroundColor Yellow
    docker-compose up -d
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Serviços iniciados com sucesso!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Acesse:" -ForegroundColor Cyan
        Write-Host "  - API Gateway: http://localhost:5000" -ForegroundColor White
        Write-Host "  - Frontend: http://localhost:3000" -ForegroundColor White
        Write-Host "  - Admin Portal: http://localhost:3001" -ForegroundColor White
        Write-Host "  - RabbitMQ Management: http://localhost:15672" -ForegroundColor White
        Write-Host "  - PostgreSQL: localhost:5432" -ForegroundColor White
        Write-Host "  - Redis: localhost:6379" -ForegroundColor White
    } else {
        Write-Host "Erro ao iniciar serviços!" -ForegroundColor Red
        exit 1
    }
}

if ($Stop) {
    Write-Host "Parando serviços..." -ForegroundColor Yellow
    docker-compose down
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Serviços parados com sucesso!" -ForegroundColor Green
    } else {
        Write-Host "Erro ao parar serviços!" -ForegroundColor Red
        exit 1
    }
}

if ($Clean) {
    Write-Host "Limpando containers e volumes..." -ForegroundColor Yellow
    docker-compose down -v --remove-orphans
    docker system prune -f
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Limpeza concluída!" -ForegroundColor Green
    } else {
        Write-Host "Erro na limpeza!" -ForegroundColor Red
        exit 1
    }
}

if ($Logs) {
    Write-Host "Exibindo logs..." -ForegroundColor Yellow
    docker-compose logs -f
}

if (-not ($Build -or $Run -or $Stop -or $Clean -or $Logs)) {
    Write-Host "Uso:" -ForegroundColor Yellow
    Write-Host "  .\build.ps1 -Build    # Construir containers" -ForegroundColor White
    Write-Host "  .\build.ps1 -Run      # Iniciar serviços" -ForegroundColor White
    Write-Host "  .\build.ps1 -Stop     # Parar serviços" -ForegroundColor White
    Write-Host "  .\build.ps1 -Clean    # Limpar tudo" -ForegroundColor White
    Write-Host "  .\build.ps1 -Logs     # Ver logs" -ForegroundColor White
    Write-Host ""
    Write-Host "Exemplo:" -ForegroundColor Yellow
    Write-Host "  .\build.ps1 -Build -Run  # Build e iniciar" -ForegroundColor White
} 