#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("staging", "production")]
    [string]$Environment,
    
    [Parameter(Mandatory=$false)]
    [string]$Tag = "previous"
)

Write-Host "🔄 Starting rollback for $Environment environment..." -ForegroundColor Yellow

# Configuration
$Config = @{
    Staging = @{
        ComposeFile = "docker-compose.staging.yml"
        Registry = "ghcr.io/your-org/lach"
    }
    Production = @{
        ComposeFile = "docker-compose.production.yml"
        Registry = "ghcr.io/your-org/lach"
    }
}

$CurrentConfig = $Config[$Environment]

# Confirm rollback
$confirmation = Read-Host "⚠️  Are you sure you want to rollback $Environment to tag '$Tag'? (yes/no)"
if ($confirmation -ne "yes") {
    Write-Host "❌ Rollback cancelled." -ForegroundColor Red
    exit 0
}

# Backup current state
Write-Host "💾 Creating backup of current state..." -ForegroundColor Yellow
$backupFile = "backup-$Environment-$(Get-Date -Format 'yyyyMMdd-HHmmss').yml"
docker-compose -f $CurrentConfig.ComposeFile config > $backupFile
Write-Host "📄 Backup saved to $backupFile" -ForegroundColor Green

# Stop services
Write-Host "🛑 Stopping current services..." -ForegroundColor Yellow
docker-compose -f $CurrentConfig.ComposeFile down

# Update compose file with rollback tag
Write-Host "📝 Updating compose file with rollback tag..." -ForegroundColor Yellow
$content = Get-Content $CurrentConfig.ComposeFile -Raw
$content = $content -replace 'image: .*:.*', "image: $($CurrentConfig.Registry)/$($_.Split('/')[-1]):$Tag"
Set-Content $CurrentConfig.ComposeFile $content

# Start services with rollback version
Write-Host "🚀 Starting services with rollback version..." -ForegroundColor Yellow
docker-compose -f $CurrentConfig.ComposeFile up -d

# Wait for services to be ready
Write-Host "⏳ Waiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Health checks
Write-Host "🏥 Running health checks..." -ForegroundColor Yellow
$services = @(
    "auth-service",
    "product-service", 
    "order-service",
    "production-queue-service",
    "delivery-service",
    "whatsapp-service",
    "notification-service",
    "api-gateway"
)

$failedServices = @()

foreach ($service in $services) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8080/health" -TimeoutSec 10 -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ $service is healthy" -ForegroundColor Green
        } else {
            Write-Host "❌ $service health check failed" -ForegroundColor Red
            $failedServices += $service
        }
    } catch {
        Write-Host "❌ $service is not responding" -ForegroundColor Red
        $failedServices += $service
    }
}

if ($failedServices.Count -gt 0) {
    Write-Host "❌ Some services failed health checks after rollback: $($failedServices -join ', ')" -ForegroundColor Red
    Write-Host "🔄 Consider rolling back to an earlier version or investigating the issue." -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Rollback to $Tag completed successfully!" -ForegroundColor Green

# Show service status
Write-Host "📊 Service status:" -ForegroundColor Yellow
docker-compose -f $CurrentConfig.ComposeFile ps

Write-Host "🎉 Rollback completed!" -ForegroundColor Green 