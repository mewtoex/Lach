using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProductService.Data;
using ProductService.Services;

namespace ProductService.Tests;

public abstract class TestBase : IDisposable
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly ProductDbContext DbContext;

    protected TestBase()
    {
        var services = new ServiceCollection();

        // Configure in-memory database
        services.AddDbContext<ProductDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Register services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IAddOnCategoryService, AddOnCategoryService>();
        services.AddScoped<IDeliveryCalculationService, DeliveryCalculationService>();
        services.AddScoped<IRecommendationService, RecommendationService>();

        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<ProductDbContext>();
        
        // Ensure database is created
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DbContext?.Dispose();
        (ServiceProvider as IDisposable)?.Dispose();
    }

    protected async Task SeedTestDataAsync()
    {
        // Add test categories
        var categories = new[]
        {
            new ProductCategoryEntity
            {
                Id = Guid.NewGuid(),
                Name = "Bebidas",
                Description = "Bebidas geladas e quentes",
                IsActive = true,
                DisplayOrder = 1
            },
            new ProductCategoryEntity
            {
                Id = Guid.NewGuid(),
                Name = "Salgados",
                Description = "Salgados diversos",
                IsActive = true,
                DisplayOrder = 2
            }
        };

        DbContext.ProductCategories.AddRange(categories);
        await DbContext.SaveChangesAsync();

        // Add test products
        var products = new[]
        {
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Coca-Cola",
                Description = "Refrigerante Coca-Cola 350ml",
                Price = 5.50m,
                CategoryId = categories[0].Id,
                IsActive = true,
                ImageUrl = "https://example.com/coca-cola.jpg"
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Coxinha",
                Description = "Coxinha de frango",
                Price = 8.00m,
                CategoryId = categories[1].Id,
                IsActive = true,
                ImageUrl = "https://example.com/coxinha.jpg"
            }
        };

        DbContext.Products.AddRange(products);
        await DbContext.SaveChangesAsync();

        // Add test add-on categories
        var addOnCategories = new[]
        {
            new AddOnCategoryEntity
            {
                Id = Guid.NewGuid(),
                Name = "Tamanhos",
                Description = "Escolha o tamanho",
                IsActive = true,
                DisplayOrder = 1,
                MaxSelections = 1,
                IsRequired = true
            },
            new AddOnCategoryEntity
            {
                Id = Guid.NewGuid(),
                Name = "Extras",
                Description = "Adicionais opcionais",
                IsActive = true,
                DisplayOrder = 2,
                MaxSelections = 3,
                IsRequired = false
            }
        };

        DbContext.AddOnCategories.AddRange(addOnCategories);
        await DbContext.SaveChangesAsync();

        // Add test add-ons
        var addOns = new[]
        {
            new ProductAddOnEntity
            {
                Id = Guid.NewGuid(),
                Name = "Pequeno",
                Description = "Tamanho pequeno",
                Price = 0.00m,
                AddOnCategoryId = addOnCategories[0].Id,
                IsActive = true,
                DisplayOrder = 1
            },
            new ProductAddOnEntity
            {
                Id = Guid.NewGuid(),
                Name = "Grande",
                Description = "Tamanho grande",
                Price = 2.00m,
                AddOnCategoryId = addOnCategories[0].Id,
                IsActive = true,
                DisplayOrder = 2
            },
            new ProductAddOnEntity
            {
                Id = Guid.NewGuid(),
                Name = "Queijo Extra",
                Description = "Queijo adicional",
                Price = 1.50m,
                AddOnCategoryId = addOnCategories[1].Id,
                IsActive = true,
                DisplayOrder = 1
            }
        };

        DbContext.ProductAddOns.AddRange(addOns);
        await DbContext.SaveChangesAsync();

        // Link products to add-on categories
        var productAddOnCategories = new[]
        {
            new ProductAddOnCategoryEntity
            {
                Id = Guid.NewGuid(),
                ProductId = products[0].Id,
                AddOnCategoryId = addOnCategories[0].Id,
                MaxSelections = 1,
                IsRequired = true,
                DisplayOrder = 1,
                IsActive = true
            },
            new ProductAddOnCategoryEntity
            {
                Id = Guid.NewGuid(),
                ProductId = products[1].Id,
                AddOnCategoryId = addOnCategories[1].Id,
                MaxSelections = 2,
                IsRequired = false,
                DisplayOrder = 1,
                IsActive = true
            }
        };

        DbContext.ProductAddOnCategories.AddRange(productAddOnCategories);
        await DbContext.SaveChangesAsync();

        // Add test store
        var store = new StoreEntity
        {
            Id = Guid.NewGuid(),
            Name = "Snack Bar Teste",
            Description = "Snack bar para testes",
            Address = "Rua Teste, 123",
            City = "São Paulo",
            State = "SP",
            ZipCode = "01234-567",
            Phone = "(11) 99999-9999",
            Email = "teste@snackbar.com",
            Cnpj = "12.345.678/0001-90",
            Latitude = -23.5505,
            Longitude = -46.6333,
            IsActive = true,
            OpeningHours = "08:00-22:00",
            DeliveryRadius = 5.0,
            BaseDeliveryFee = 3.00m,
            PricePerKm = 1.50m,
            FreeDeliveryThreshold = 30.00m,
            MaxDeliveryDistance = 10.0
        };

        DbContext.Stores.Add(store);
        await DbContext.SaveChangesAsync();
    }
} 