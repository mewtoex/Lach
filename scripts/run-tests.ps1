#!/usr/bin/env pwsh

# Script para executar todos os testes do projeto Lach...
param(
    [switch]$Coverage,
    [switch]$Verbose,
    [string]$Filter = ""
)

Write-Host "🚀 Executando testes do projeto Lach..." -ForegroundColor Green

# Configurações
$TestProjects = @(
    "tests/ProductService.Tests",
    "tests/OrderService.Tests", 
    "tests/AuthService.Tests"
)

$Results = @()
$TotalTests = 0
$PassedTests = 0
$FailedTests = 0

# Função para executar testes de um projeto
function Run-TestProject {
    param(
        [string]$ProjectPath,
        [string]$ProjectName
    )
    
    Write-Host "`n📋 Executando testes do $ProjectName..." -ForegroundColor Yellow
    
    $testArgs = @("test", $ProjectPath, "--verbosity", "normal")
    
    if ($Coverage) {
        $testArgs += @("--collect", "XPlat Code Coverage")
    }
    
    if ($Filter) {
        $testArgs += @("--filter", $Filter)
    }
    
    if ($Verbose) {
        $testArgs += @("--logger", "console;verbosity=detailed")
    }
    
    try {
        $output = & dotnet $testArgs 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ $ProjectName: Todos os testes passaram!" -ForegroundColor Green
            
            # Extrair número de testes
            $testLine = $output | Where-Object { $_ -match "Total:\s*(\d+)" }
            if ($testLine) {
                $testCount = [int]($testLine -replace ".*Total:\s*(\d+).*", '$1')
                $TotalTests += $testCount
                $PassedTests += $testCount
            }
        } else {
            Write-Host "❌ $ProjectName: Alguns testes falharam!" -ForegroundColor Red
            
            # Extrair número de testes
            $testLine = $output | Where-Object { $_ -match "Total:\s*(\d+)" }
            if ($testLine) {
                $testCount = [int]($testLine -replace ".*Total:\s*(\d+).*", '$1')
                $TotalTests += $testCount
            }
            
            $failedLine = $output | Where-Object { $_ -match "Failed:\s*(\d+)" }
            if ($failedLine) {
                $failedCount = [int]($failedLine -replace ".*Failed:\s*(\d+).*", '$1')
                $FailedTests += $failedCount
                $PassedTests += ($testCount - $failedCount)
            }
        }
        
        $Results += @{
            Project = $ProjectName
            Success = ($LASTEXITCODE -eq 0)
            Output = $output
        }
        
    } catch {
        Write-Host "❌ Erro ao executar testes do $ProjectName: $_" -ForegroundColor Red
        $Results += @{
            Project = $ProjectName
            Success = $false
            Output = @("Erro: $_")
        }
    }
}

# Executar testes para cada projeto
foreach ($project in $TestProjects) {
    if (Test-Path $project) {
        $projectName = Split-Path $project -Leaf
        Run-TestProject -ProjectPath $project -ProjectName $projectName
    } else {
        Write-Host "⚠️  Projeto de teste não encontrado: $project" -ForegroundColor Yellow
    }
}

# Resumo final
Write-Host "`n📊 Resumo dos Testes:" -ForegroundColor Cyan
Write-Host "==================" -ForegroundColor Cyan

foreach ($result in $Results) {
    $status = if ($result.Success) { "✅" } else { "❌" }
    Write-Host "$status $($result.Project)" -ForegroundColor $(if ($result.Success) { "Green" } else { "Red" })
}

Write-Host "`n📈 Estatísticas:" -ForegroundColor Cyan
Write-Host "Total de testes: $TotalTests" -ForegroundColor White
Write-Host "Testes aprovados: $PassedTests" -ForegroundColor Green
Write-Host "Testes falhados: $FailedTests" -ForegroundColor Red

if ($TotalTests -gt 0) {
    $successRate = [math]::Round(($PassedTests / $TotalTests) * 100, 2)
    Write-Host "Taxa de sucesso: $successRate%" -ForegroundColor $(if ($successRate -ge 80) { "Green" } elseif ($successRate -ge 60) { "Yellow" } else { "Red" })
}

# Verificar se todos os testes passaram
$allPassed = $Results | Where-Object { $_.Success } | Measure-Object | Select-Object -ExpandProperty Count
$totalProjects = $Results.Count

if ($allPassed -eq $totalProjects -and $totalProjects -gt 0) {
    Write-Host "`n🎉 Todos os testes passaram com sucesso!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n💥 Alguns testes falharam. Verifique os detalhes acima." -ForegroundColor Red
    exit 1
} 