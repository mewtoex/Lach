# Lach System - Test Runner Script
# Executa todos os testes unitários e de integração

param(
    [switch]$Unit,
    [switch]$Integration,
    [switch]$All,
    [switch]$Coverage,
    [string]$Service = ""
)

Write-Host "🧪 Lach System - Test Runner" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

# Default to all tests if no specific type is specified
if (-not $Unit -and -not $Integration -and -not $All) {
    $All = $true
}

# Test projects
$testProjects = @(
    "src/Tests/AuthService.Tests/AuthService.Tests.csproj",
    "src/Tests/OrderService.Tests/OrderService.Tests.csproj",
    "src/Tests/ProductionQueueService.Tests/ProductionQueueService.Tests.csproj"
)

# Filter by service if specified
if ($Service -ne "") {
    $testProjects = $testProjects | Where-Object { $_ -like "*$Service*" }
}

$totalTests = 0
$passedTests = 0
$failedTests = 0

foreach ($project in $testProjects) {
    if (Test-Path $project) {
        Write-Host "`n📋 Running tests for: $project" -ForegroundColor Yellow
        
        $projectName = Split-Path (Split-Path $project -Parent) -Leaf
        $coveragePath = "coverage/$projectName"
        
        # Create coverage directory
        if ($Coverage) {
            New-Item -ItemType Directory -Force -Path $coveragePath | Out-Null
        }
        
        # Build arguments
        $args = @("test", $project, "--verbosity", "normal")
        
        if ($Coverage) {
            $args += @(
                "/p:CollectCoverage=true",
                "/p:CoverletOutputFormat=opencover",
                "/p:CoverletOutput=$coveragePath/coverage.xml",
                "/p:Exclude=`"[*]*.Program,[*]*.Startup`""
            )
        }
        
        # Run tests
        try {
            $result = & dotnet $args
            
            # Parse results
            $output = $result -join "`n"
            if ($output -match "Total:\s*(\d+), Errors:\s*(\d+), Failed:\s*(\d+), Skipped:\s*(\d+), Passed:\s*(\d+)") {
                $total = [int]$matches[1]
                $errors = [int]$matches[2]
                $failed = [int]$matches[3]
                $skipped = [int]$matches[4]
                $passed = [int]$matches[5]
                
                $totalTests += $total
                $passedTests += $passed
                $failedTests += ($failed + $errors)
                
                if ($failed -eq 0 -and $errors -eq 0) {
                    Write-Host "✅ $projectName: $passed passed, $skipped skipped" -ForegroundColor Green
                } else {
                    Write-Host "❌ $projectName: $failed failed, $errors errors, $passed passed, $skipped skipped" -ForegroundColor Red
                }
            }
            
            # Show coverage if enabled
            if ($Coverage -and (Test-Path "$coveragePath/coverage.xml")) {
                Write-Host "📊 Coverage report generated: $coveragePath/coverage.xml" -ForegroundColor Blue
            }
            
        } catch {
            Write-Host "❌ Error running tests for $projectName: $_" -ForegroundColor Red
            $failedTests++
        }
    } else {
        Write-Host "⚠️  Test project not found: $project" -ForegroundColor Yellow
    }
}

# Summary
Write-Host "`n📊 Test Summary" -ForegroundColor Cyan
Write-Host "===============" -ForegroundColor Cyan
Write-Host "Total Tests: $totalTests" -ForegroundColor White
Write-Host "Passed: $passedTests" -ForegroundColor Green
Write-Host "Failed: $failedTests" -ForegroundColor Red

if ($failedTests -eq 0) {
    Write-Host "`n🎉 All tests passed!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n💥 Some tests failed!" -ForegroundColor Red
    exit 1
} 