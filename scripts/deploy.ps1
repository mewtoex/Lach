#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("local", "staging", "production")]
    [string]$Environment,
    
    [Parameter(Mandatory=$false)]
    [string]$Tag = "latest",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTests,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild
)

Write-Host "🚀 Starting deployment to $Environment environment..." -ForegroundColor Green

# Configuration
$Config = @{
    Local = @{
        ComposeFile = "docker-compose.yml"
        Registry = ""
        Tag = "latest"
    }
    Staging = @{
        ComposeFile = "docker-compose.staging.yml"
        Registry = "ghcr.io/your-org/lach"
        Tag = $Tag
    }
    Production = @{
        ComposeFile = "docker-compose.production.yml"
        Registry = "ghcr.io/your-org/lach"
        Tag = $Tag
    }
}

$CurrentConfig = $Config[$Environment]

# Pre-deployment checks
Write-Host "📋 Running pre-deployment checks..." -ForegroundColor Yellow

if (-not $SkipTests) {
    Write-Host "🧪 Running tests..." -ForegroundColor Yellow
    dotnet test --configuration Release --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Tests failed. Aborting deployment." -ForegroundColor Red
        exit 1
    }
}

if (-not $SkipBuild) {
    Write-Host "🔨 Building services..." -ForegroundColor Yellow
    dotnet build --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed. Aborting deployment." -ForegroundColor Red
        exit 1
    }
}

# Environment-specific setup
switch ($Environment) {
    "local" {
        Write-Host "🏠 Setting up local environment..." -ForegroundColor Yellow
        
        # Check if Docker is running
        try {
            docker version | Out-Null
        } catch {
            Write-Host "❌ Docker is not running. Please start Docker Desktop." -ForegroundColor Red
            exit 1
        }
        
        # Create .env file if it doesn't exist
        if (-not (Test-Path ".env")) {
            Write-Host "📝 Creating .env file..." -ForegroundColor Yellow
            Copy-Item "env.example" ".env"
        }
    }
    
    "staging" {
        Write-Host "🧪 Setting up staging environment..." -ForegroundColor Yellow
        
        # Login to container registry
        Write-Host "🔐 Logging in to container registry..." -ForegroundColor Yellow
        docker login ghcr.io -u $env:GITHUB_USERNAME -p $env:GITHUB_TOKEN
        
        # Create staging compose file
        if (-not (Test-Path $CurrentConfig.ComposeFile)) {
            Write-Host "📝 Creating staging compose file..." -ForegroundColor Yellow
            Copy-Item "docker-compose.yml" $CurrentConfig.ComposeFile
            (Get-Content $CurrentConfig.ComposeFile) -replace 'latest', $CurrentConfig.Tag | Set-Content $CurrentConfig.ComposeFile
        }
    }
    
    "production" {
        Write-Host "🚀 Setting up production environment..." -ForegroundColor Yellow
        
        # Login to container registry
        Write-Host "🔐 Logging in to container registry..." -ForegroundColor Yellow
        docker login ghcr.io -u $env:GITHUB_USERNAME -p $env:GITHUB_TOKEN
        
        # Create production compose file
        if (-not (Test-Path $CurrentConfig.ComposeFile)) {
            Write-Host "📝 Creating production compose file..." -ForegroundColor Yellow
            Copy-Item "docker-compose.yml" $CurrentConfig.ComposeFile
            (Get-Content $CurrentConfig.ComposeFile) -replace 'latest', $CurrentConfig.Tag | Set-Content $CurrentConfig.ComposeFile
        }
        
        # Confirm production deployment
        $confirmation = Read-Host "⚠️  Are you sure you want to deploy to PRODUCTION? (yes/no)"
        if ($confirmation -ne "yes") {
            Write-Host "❌ Production deployment cancelled." -ForegroundColor Red
            exit 0
        }
    }
}

# Stop existing services
Write-Host "🛑 Stopping existing services..." -ForegroundColor Yellow
docker-compose -f $CurrentConfig.ComposeFile down --remove-orphans

# Pull latest images (for staging/production)
if ($Environment -ne "local") {
    Write-Host "📥 Pulling latest images..." -ForegroundColor Yellow
    docker-compose -f $CurrentConfig.ComposeFile pull
}

# Start services
Write-Host "🚀 Starting services..." -ForegroundColor Yellow
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
    Write-Host "❌ Some services failed health checks: $($failedServices -join ', ')" -ForegroundColor Red
    Write-Host "📋 Checking service logs..." -ForegroundColor Yellow
    
    foreach ($service in $failedServices) {
        Write-Host "📄 Logs for $service:" -ForegroundColor Yellow
        docker-compose -f $CurrentConfig.ComposeFile logs --tail=50 $service
    }
    
    exit 1
}

# Post-deployment tasks
Write-Host "🔧 Running post-deployment tasks..." -ForegroundColor Yellow

# Run database migrations
Write-Host "🗄️  Running database migrations..." -ForegroundColor Yellow
docker-compose -f $CurrentConfig.ComposeFile exec -T auth-service dotnet ef database update
docker-compose -f $CurrentConfig.ComposeFile exec -T product-service dotnet ef database update
docker-compose -f $CurrentConfig.ComposeFile exec -T order-service dotnet ef database update
docker-compose -f $CurrentConfig.ComposeFile exec -T production-queue-service dotnet ef database update
docker-compose -f $CurrentConfig.ComposeFile exec -T delivery-service dotnet ef database update
docker-compose -f $CurrentConfig.ComposeFile exec -T whatsapp-service dotnet ef database update

# Seed data if local environment
if ($Environment -eq "local") {
    Write-Host "🌱 Seeding initial data..." -ForegroundColor Yellow
    docker-compose -f $CurrentConfig.ComposeFile exec -T product-service dotnet run --project src/Services/ProductService/SeedData.csproj
}

Write-Host "✅ Deployment to $Environment completed successfully!" -ForegroundColor Green

# Show service status
Write-Host "📊 Service status:" -ForegroundColor Yellow
docker-compose -f $CurrentConfig.ComposeFile ps

# Show access information
Write-Host "🌐 Access information:" -ForegroundColor Yellow
Write-Host "   API Gateway: http://localhost:8080" -ForegroundColor Cyan
Write-Host "   Grafana: http://localhost:3000" -ForegroundColor Cyan
Write-Host "   Prometheus: http://localhost:9090" -ForegroundColor Cyan
Write-Host "   RabbitMQ Management: http://localhost:15672" -ForegroundColor Cyan
Write-Host "   PostgreSQL: localhost:5432" -ForegroundColor Cyan

Write-Host "🎉 Deployment completed!" -ForegroundColor Green 