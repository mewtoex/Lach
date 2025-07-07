# Lach System - Load Test Script
# Executa testes de carga usando NBomber

param(
    [string]$BaseUrl = "http://localhost:5000",
    [int]$Duration = 60,
    [int]$Rate = 100,
    [string]$Scenario = "all"
)

Write-Host "🚀 Lach System - Load Testing" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan

# Check if NBomber is installed
$nbomberPath = Get-Command nbomber -ErrorAction SilentlyContinue
if (-not $nbomberPath) {
    Write-Host "📦 Installing NBomber..." -ForegroundColor Yellow
    dotnet tool install --global NBomber
}

# Create load test project if it doesn't exist
$loadTestProject = "src/Tests/LoadTests/LoadTests.csproj"
if (-not (Test-Path $loadTestProject)) {
    Write-Host "📁 Creating load test project..." -ForegroundColor Yellow
    
    # Create directory
    New-Item -ItemType Directory -Force -Path "src/Tests/LoadTests" | Out-Null
    
    # Create project file
    $projectContent = @"
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="NBomber" Version="4.4.0" />
    <PackageReference Include="NBomber.Http" Version="4.4.0" />
    <PackageReference Include="NBomber.Plugins.Http" Version="4.4.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../../../Shared/Lach.Shared.Common/Lach.Shared.Common.csproj" />
  </ItemGroup>

</Project>
"@
    
    Set-Content -Path $loadTestProject -Value $projectContent
    
    # Create load test scenarios
    $loadTestFile = "src/Tests/LoadTests/LoadTestScenarios.cs"
    $scenariosContent = @"
using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using Lach.Shared.Common.Models;

namespace LoadTests;

public class LoadTestScenarios
{
    private static string _baseUrl = "$BaseUrl";
    private static string _authToken = "";

    [Fact]
    public void Auth_Load_Test()
    {
        var httpClient = new HttpClient();

        var scenario = Scenario.Create("auth_load_test", async context =>
        {
            var request = Http.CreateRequest("POST", $"{_baseUrl}/api/auth/register")
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonContent.Create(new RegisterRequest
                {
                    Name = $"Test User {Guid.NewGuid()}",
                    Email = $"test{Guid.NewGuid()}@example.com",
                    Phone = "+5511999999999",
                    Password = "password123",
                    Role = UserRole.Customer
                }));

            var response = await Http.Send(httpClient, request);
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: $Rate, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds($Duration))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }

    [Fact]
    public void Product_Load_Test()
    {
        var httpClient = new HttpClient();

        var scenario = Scenario.Create("product_load_test", async context =>
        {
            var request = Http.CreateRequest("GET", $"{_baseUrl}/api/products")
                .WithHeader("Accept", "application/json");

            var response = await Http.Send(httpClient, request);
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: $Rate, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds($Duration))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }

    [Fact]
    public void Order_Load_Test()
    {
        var httpClient = new HttpClient();

        var scenario = Scenario.Create("order_load_test", async context =>
        {
            var request = Http.CreateRequest("POST", $"{_baseUrl}/api/orders")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Authorization", $"Bearer {_authToken}")
                .WithBody(JsonContent.Create(new CreateOrderRequest
                {
                    CustomerId = Guid.NewGuid(),
                    CustomerName = $"Test Customer {Guid.NewGuid()}",
                    CustomerEmail = $"customer{Guid.NewGuid()}@example.com",
                    CustomerPhone = "+5511999999999",
                    Items = new List<OrderItemRequest>
                    {
                        new()
                        {
                            ProductId = Guid.NewGuid(),
                            ProductName = "X-Burger",
                            Quantity = 2,
                            Price = 15.90m
                        }
                    },
                    DeliveryAddress = "Rua das Flores, 123",
                    TotalAmount = 31.80m
                }));

            var response = await Http.Send(httpClient, request);
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: $Rate, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds($Duration))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }

    [Fact]
    public void Full_System_Load_Test()
    {
        var httpClient = new HttpClient();

        // Register user first
        var registerRequest = Http.CreateRequest("POST", $"{_baseUrl}/api/auth/register")
            .WithHeader("Content-Type", "application/json")
            .WithBody(JsonContent.Create(new RegisterRequest
            {
                Name = "Load Test User",
                Email = $"loadtest{Guid.NewGuid()}@example.com",
                Phone = "+5511999999999",
                Password = "password123",
                Role = UserRole.Customer
            }));

        var registerResponse = Http.Send(httpClient, registerRequest).Result;
        if (registerResponse.IsSuccessStatusCode)
        {
            var loginRequest = Http.CreateRequest("POST", $"{_baseUrl}/api/auth/login")
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonContent.Create(new LoginRequest
                {
                    Email = "loadtest@example.com",
                    Password = "password123"
                }));

            var loginResponse = Http.Send(httpClient, loginRequest).Result;
            if (loginResponse.IsSuccessStatusCode)
            {
                // Extract token from response (simplified)
                _authToken = "test-token";
            }
        }

        var scenario = Scenario.Create("full_system_load_test", async context =>
        {
            // Randomly choose an action
            var action = context.Random.Next(1, 4);
            
            HttpResponseMessage response;
            
            switch (action)
            {
                case 1:
                    // Get products
                    var productRequest = Http.CreateRequest("GET", $"{_baseUrl}/api/products");
                    response = await Http.Send(httpClient, productRequest);
                    break;
                    
                case 2:
                    // Create order
                    var orderRequest = Http.CreateRequest("POST", $"{_baseUrl}/api/orders")
                        .WithHeader("Content-Type", "application/json")
                        .WithHeader("Authorization", $"Bearer {_authToken}")
                        .WithBody(JsonContent.Create(new CreateOrderRequest
                        {
                            CustomerId = Guid.NewGuid(),
                            CustomerName = $"Load Test Customer {Guid.NewGuid()}",
                            CustomerEmail = $"loadtest{Guid.NewGuid()}@example.com",
                            CustomerPhone = "+5511999999999",
                            Items = new List<OrderItemRequest>
                            {
                                new()
                                {
                                    ProductId = Guid.NewGuid(),
                                    ProductName = "X-Burger",
                                    Quantity = 1,
                                    Price = 15.90m
                                }
                            },
                            DeliveryAddress = "Rua das Flores, 123",
                            TotalAmount = 15.90m
                        }));
                    response = await Http.Send(httpClient, orderRequest);
                    break;
                    
                default:
                    // Health check
                    var healthRequest = Http.CreateRequest("GET", $"{_baseUrl}/health");
                    response = await Http.Send(httpClient, healthRequest);
                    break;
            }
            
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: $Rate, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds($Duration))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }
}
"@
    
    Set-Content -Path $loadTestFile -Value $scenariosContent
}

# Run load tests
Write-Host "`n🎯 Running load tests..." -ForegroundColor Yellow
Write-Host "Base URL: $BaseUrl" -ForegroundColor White
Write-Host "Duration: $Duration seconds" -ForegroundColor White
Write-Host "Rate: $Rate requests/second" -ForegroundColor White

try {
    switch ($Scenario.ToLower()) {
        "auth" {
            Write-Host "`n🔐 Testing Authentication endpoints..." -ForegroundColor Blue
            dotnet test $loadTestProject --filter "Auth_Load_Test"
        }
        "product" {
            Write-Host "`n🍔 Testing Product endpoints..." -ForegroundColor Blue
            dotnet test $loadTestProject --filter "Product_Load_Test"
        }
        "order" {
            Write-Host "`n📦 Testing Order endpoints..." -ForegroundColor Blue
            dotnet test $loadTestProject --filter "Order_Load_Test"
        }
        "full" {
            Write-Host "`n🚀 Testing Full System..." -ForegroundColor Blue
            dotnet test $loadTestProject --filter "Full_System_Load_Test"
        }
        default {
            Write-Host "`n🎯 Testing All Scenarios..." -ForegroundColor Blue
            dotnet test $loadTestProject
        }
    }
    
    Write-Host "`n✅ Load tests completed!" -ForegroundColor Green
    Write-Host "📊 Check the reports in the test output above." -ForegroundColor Blue
    
} catch {
    Write-Host "`n❌ Load tests failed: $_" -ForegroundColor Red
    exit 1
} 