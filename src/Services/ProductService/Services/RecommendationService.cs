using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductService.Data;
using ProductService.Models;
using System.Text.Json;

namespace ProductService.Services;

public class RecommendationService : IRecommendationService
{
    private readonly ProductDbContext _context;
    private readonly ILogger<RecommendationService> _logger;
    private readonly Dictionary<Guid, decimal> _productPopularityScores = new();
    private readonly Dictionary<Guid, List<Guid>> _productSimilarities = new();

    public RecommendationService(ProductDbContext context, ILogger<RecommendationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProductRecommendationResponse> GetRecommendationsAsync(ProductRecommendationRequest request)
    {
        try
        {
            var recommendations = new List<ProductSummaryResponse>();
            var reason = "Baseado em popularidade geral";
            var confidenceScore = 0.7m;

            // 1. Personalized recommendations if customer ID is provided
            if (request.CustomerId.HasValue)
            {
                var personalized = await GetPersonalizedRecommendationsAsync(request.CustomerId.Value, request.Limit);
                if (personalized.Any())
                {
                    recommendations.AddRange(personalized);
                    reason = "Baseado no seu histórico de pedidos";
                    confidenceScore = 0.9m;
                }
            }

            // 2. Category-based recommendations
            if (request.CategoryId.HasValue && recommendations.Count < request.Limit)
            {
                var categoryProducts = await GetProductsByCategoryAsync(request.CategoryId.Value, request.Limit - recommendations.Count);
                recommendations.AddRange(categoryProducts);
                if (recommendations.Count > 0 && reason == "Baseado em popularidade geral")
                {
                    reason = "Baseado na categoria selecionada";
                    confidenceScore = 0.8m;
                }
            }

            // 3. Similar products based on cart items
            if (request.CartItems?.Any() == true && recommendations.Count < request.Limit)
            {
                var similarProducts = await GetSimilarProductsForCartAsync(request.CartItems, request.Limit - recommendations.Count);
                recommendations.AddRange(similarProducts);
                if (recommendations.Count > 0 && reason == "Baseado em popularidade geral")
                {
                    reason = "Baseado nos itens do seu carrinho";
                    confidenceScore = 0.85m;
                }
            }

            // 4. Recently viewed products
            if (request.RecentlyViewedProducts?.Any() == true && recommendations.Count < request.Limit)
            {
                var recentlyViewed = await GetSimilarProductsForRecentlyViewedAsync(request.RecentlyViewedProducts, request.Limit - recommendations.Count);
                recommendations.AddRange(recentlyViewed);
                if (recommendations.Count > 0 && reason == "Baseado em popularidade geral")
                {
                    reason = "Baseado nos produtos que você visualizou";
                    confidenceScore = 0.8m;
                }
            }

            // 5. Fill with popular products if needed
            if (recommendations.Count < request.Limit)
            {
                var popularProducts = await GetPopularProductsAsync(request.Limit - recommendations.Count);
                recommendations.AddRange(popularProducts);
            }

            // Remove duplicates and limit results
            recommendations = recommendations
                .GroupBy(p => p.Id)
                .Select(g => g.First())
                .Take(request.Limit)
                .ToList();

            return new ProductRecommendationResponse
            {
                RecommendedProducts = recommendations,
                RecommendationReason = reason,
                ConfidenceScore = confidenceScore
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar recomendações");
            return new ProductRecommendationResponse
            {
                RecommendedProducts = await GetPopularProductsAsync(request.Limit),
                RecommendationReason = "Produtos populares (fallback)",
                ConfidenceScore = 0.5m
            };
        }
    }

    public async Task<List<ProductSummaryResponse>> GetPopularProductsAsync(int limit = 10)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.AddOns)
            .Where(p => p.IsAvailable)
            .OrderByDescending(p => p.Metadata != null && p.Metadata.ContainsKey("popularity_score") 
                ? JsonSerializer.Deserialize<decimal>(p.Metadata["popularity_score"].ToString() ?? "0") 
                : 0)
            .ThenByDescending(p => p.CreatedAt)
            .Take(limit)
            .Select(p => new ProductSummaryResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                IsAvailable = p.IsAvailable,
                ImageUrl = p.ImageUrl,
                PreparationTimeMinutes = p.PreparationTimeMinutes,
                HasAddOns = p.HasAddOns,
                AddOnsCount = p.AddOns.Count,
                Category = new CategorySummaryResponse
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name,
                    Description = p.Category.Description,
                    ImageUrl = p.Category.ImageUrl,
                    Color = p.Category.Color,
                    Icon = p.Category.Icon,
                    IsActive = p.Category.IsActive,
                    DisplayOrder = p.Category.DisplayOrder,
                    ProductsCount = p.Category.Products.Count
                },
                Stats = new ProductStats
                {
                    TotalOrders = p.Metadata != null && p.Metadata.ContainsKey("total_orders") 
                        ? JsonSerializer.Deserialize<int>(p.Metadata["total_orders"].ToString() ?? "0") 
                        : 0,
                    PopularityScore = p.Metadata != null && p.Metadata.ContainsKey("popularity_score") 
                        ? JsonSerializer.Deserialize<decimal>(p.Metadata["popularity_score"].ToString() ?? "0") 
                        : 0
                },
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return products;
    }

    public async Task<List<ProductSummaryResponse>> GetTrendingProductsAsync(int limit = 10)
    {
        // Implementar lógica de produtos em tendência baseada em:
        // - Pedidos recentes (últimas 24h)
        // - Visualizações recentes
        // - Crescimento de popularidade
        
        var trendingProducts = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.AddOns)
            .Where(p => p.IsAvailable)
            .OrderByDescending(p => p.Metadata != null && p.Metadata.ContainsKey("trending_score") 
                ? JsonSerializer.Deserialize<decimal>(p.Metadata["trending_score"].ToString() ?? "0") 
                : 0)
            .ThenByDescending(p => p.CreatedAt)
            .Take(limit)
            .Select(p => new ProductSummaryResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                IsAvailable = p.IsAvailable,
                ImageUrl = p.ImageUrl,
                PreparationTimeMinutes = p.PreparationTimeMinutes,
                HasAddOns = p.HasAddOns,
                AddOnsCount = p.AddOns.Count,
                Category = new CategorySummaryResponse
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name,
                    Description = p.Category.Description,
                    ImageUrl = p.Category.ImageUrl,
                    Color = p.Category.Color,
                    Icon = p.Category.Icon,
                    IsActive = p.Category.IsActive,
                    DisplayOrder = p.Category.DisplayOrder,
                    ProductsCount = p.Category.Products.Count
                },
                Stats = new ProductStats
                {
                    PopularityScore = p.Metadata != null && p.Metadata.ContainsKey("trending_score") 
                        ? JsonSerializer.Deserialize<decimal>(p.Metadata["trending_score"].ToString() ?? "0") 
                        : 0
                },
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return trendingProducts;
    }

    public async Task<List<ProductSummaryResponse>> GetSimilarProductsAsync(Guid productId, int limit = 5)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.AddOns)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
            return new List<ProductSummaryResponse>();

        // Buscar produtos similares baseado em:
        // - Mesma categoria
        // - Preço similar
        // - Adicionais similares
        // - Padrões de compra (frequentemente comprados juntos)

        var similarProducts = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.AddOns)
            .Where(p => p.Id != productId && p.IsAvailable)
            .Where(p => p.CategoryId == product.CategoryId || 
                       Math.Abs((double)(p.Price - product.Price)) < 5.0 ||
                       p.AddOns.Any(a => product.AddOns.Any(pa => pa.Category == a.Category)))
            .OrderByDescending(p => p.Metadata != null && p.Metadata.ContainsKey("similarity_score") 
                ? JsonSerializer.Deserialize<decimal>(p.Metadata["similarity_score"].ToString() ?? "0") 
                : 0)
            .Take(limit)
            .Select(p => new ProductSummaryResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                IsAvailable = p.IsAvailable,
                ImageUrl = p.ImageUrl,
                PreparationTimeMinutes = p.PreparationTimeMinutes,
                HasAddOns = p.HasAddOns,
                AddOnsCount = p.AddOns.Count,
                Category = new CategorySummaryResponse
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name,
                    Description = p.Category.Description,
                    ImageUrl = p.Category.ImageUrl,
                    Color = p.Category.Color,
                    Icon = p.Category.Icon,
                    IsActive = p.Category.IsActive,
                    DisplayOrder = p.Category.DisplayOrder,
                    ProductsCount = p.Category.Products.Count
                },
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return similarProducts;
    }

    public async Task<List<ProductSummaryResponse>> GetPersonalizedRecommendationsAsync(Guid customerId, int limit = 10)
    {
        // Implementar recomendações personalizadas baseadas em:
        // - Histórico de pedidos do cliente
        // - Produtos favoritos
        // - Padrões de compra
        // - Avaliações do cliente
        
        // Por enquanto, retornar produtos populares da categoria mais comprada
        var popularProducts = await GetPopularProductsAsync(limit);
        
        // TODO: Implementar lógica de ML mais sofisticada
        // - Collaborative filtering
        // - Content-based filtering
        // - Matrix factorization
        
        return popularProducts;
    }

    public async Task<List<CategorySummaryResponse>> GetRecommendedCategoriesAsync(Guid customerId, int limit = 5)
    {
        // Implementar recomendações de categorias baseadas no histórico do cliente
        var categories = await _context.Categories
            .Include(c => c.Products)
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.Products.Count(p => p.IsAvailable))
            .Take(limit)
            .Select(c => new CategorySummaryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                Color = c.Color,
                Icon = c.Icon,
                IsActive = c.IsActive,
                DisplayOrder = c.DisplayOrder,
                ProductsCount = c.Products.Count(p => p.IsAvailable)
            })
            .ToListAsync();

        return categories;
    }

    public async Task UpdateProductPopularityAsync(Guid productId, int viewCount = 1, int orderCount = 0)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return;

            // Atualizar métricas de popularidade
            var metadata = product.Metadata ?? new Dictionary<string, object>();
            
            var currentViews = metadata.ContainsKey("view_count") 
                ? JsonSerializer.Deserialize<int>(metadata["view_count"].ToString() ?? "0") 
                : 0;
            var currentOrders = metadata.ContainsKey("total_orders") 
                ? JsonSerializer.Deserialize<int>(metadata["total_orders"].ToString() ?? "0") 
                : 0;
            var currentPopularity = metadata.ContainsKey("popularity_score") 
                ? JsonSerializer.Deserialize<decimal>(metadata["popularity_score"].ToString() ?? "0") 
                : 0;

            // Calcular novo score de popularidade
            var newViews = currentViews + viewCount;
            var newOrders = currentOrders + orderCount;
            var newPopularity = CalculatePopularityScore(newViews, newOrders, product.CreatedAt);

            metadata["view_count"] = newViews;
            metadata["total_orders"] = newOrders;
            metadata["popularity_score"] = newPopularity;
            metadata["last_updated"] = DateTime.UtcNow;

            product.Metadata = metadata;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar popularidade do produto {ProductId}", productId);
        }
    }

    public async Task TrainRecommendationModelAsync()
    {
        try
        {
            _logger.LogInformation("Iniciando treinamento do modelo de recomendações");

            // 1. Coletar dados de treinamento
            var trainingData = await CollectTrainingDataAsync();

            // 2. Calcular similaridades entre produtos
            await CalculateProductSimilaritiesAsync(trainingData);

            // 3. Atualizar scores de popularidade
            await UpdateAllPopularityScoresAsync();

            // 4. Salvar modelo treinado
            await SaveTrainedModelAsync();

            _logger.LogInformation("Modelo de recomendações treinado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao treinar modelo de recomendações");
        }
    }

    public async Task<Dictionary<string, object>> GetRecommendationMetricsAsync()
    {
        var metrics = new Dictionary<string, object>();

        try
        {
            // Métricas de popularidade
            var totalProducts = await _context.Products.CountAsync();
            var productsWithMetadata = await _context.Products
                .Where(p => p.Metadata != null && p.Metadata.ContainsKey("popularity_score"))
                .CountAsync();

            metrics["total_products"] = totalProducts;
            metrics["products_with_metadata"] = productsWithMetadata;
            metrics["coverage_percentage"] = totalProducts > 0 ? (decimal)productsWithMetadata / totalProducts * 100 : 0;

            // Top produtos mais populares
            var topProducts = await _context.Products
                .Where(p => p.Metadata != null && p.Metadata.ContainsKey("popularity_score"))
                .OrderByDescending(p => JsonSerializer.Deserialize<decimal>(p.Metadata["popularity_score"].ToString() ?? "0"))
                .Take(5)
                .Select(p => new { p.Name, PopularityScore = JsonSerializer.Deserialize<decimal>(p.Metadata["popularity_score"].ToString() ?? "0") })
                .ToListAsync();

            metrics["top_products"] = topProducts;

            // Métricas de categoria
            var categoryStats = await _context.Categories
                .Include(c => c.Products)
                .Select(c => new
                {
                    c.Name,
                    ProductsCount = c.Products.Count,
                    AveragePopularity = c.Products
                        .Where(p => p.Metadata != null && p.Metadata.ContainsKey("popularity_score"))
                        .Select(p => JsonSerializer.Deserialize<decimal>(p.Metadata["popularity_score"].ToString() ?? "0"))
                        .DefaultIfEmpty(0)
                        .Average()
                })
                .ToListAsync();

            metrics["category_stats"] = categoryStats;

            // Última atualização do modelo
            var lastUpdate = await _context.Products
                .Where(p => p.Metadata != null && p.Metadata.ContainsKey("last_updated"))
                .OrderByDescending(p => JsonSerializer.Deserialize<DateTime>(p.Metadata["last_updated"].ToString() ?? DateTime.MinValue.ToString()))
                .Select(p => JsonSerializer.Deserialize<DateTime>(p.Metadata["last_updated"].ToString() ?? DateTime.MinValue.ToString()))
                .FirstOrDefaultAsync();

            metrics["last_model_update"] = lastUpdate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter métricas de recomendações");
            metrics["error"] = ex.Message;
        }

        return metrics;
    }

    #region Private Methods

    private async Task<List<ProductSummaryResponse>> GetProductsByCategoryAsync(Guid categoryId, int limit)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.AddOns)
            .Where(p => p.CategoryId == categoryId && p.IsAvailable)
            .OrderByDescending(p => p.Metadata != null && p.Metadata.ContainsKey("popularity_score") 
                ? JsonSerializer.Deserialize<decimal>(p.Metadata["popularity_score"].ToString() ?? "0") 
                : 0)
            .Take(limit)
            .Select(p => new ProductSummaryResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                IsAvailable = p.IsAvailable,
                ImageUrl = p.ImageUrl,
                PreparationTimeMinutes = p.PreparationTimeMinutes,
                HasAddOns = p.HasAddOns,
                AddOnsCount = p.AddOns.Count,
                Category = new CategorySummaryResponse
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name,
                    Description = p.Category.Description,
                    ImageUrl = p.Category.ImageUrl,
                    Color = p.Category.Color,
                    Icon = p.Category.Icon,
                    IsActive = p.Category.IsActive,
                    DisplayOrder = p.Category.DisplayOrder,
                    ProductsCount = p.Category.Products.Count
                },
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    private async Task<List<ProductSummaryResponse>> GetSimilarProductsForCartAsync(List<Guid> cartItems, int limit)
    {
        // Implementar lógica para encontrar produtos similares aos itens do carrinho
        var cartProducts = await _context.Products
            .Include(p => p.Category)
            .Where(p => cartItems.Contains(p.Id))
            .ToListAsync();

        if (!cartProducts.Any())
            return new List<ProductSummaryResponse>();

        var categoryIds = cartProducts.Select(p => p.CategoryId).Distinct().ToList();
        
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.AddOns)
            .Where(p => !cartItems.Contains(p.Id) && p.IsAvailable && categoryIds.Contains(p.CategoryId))
            .OrderByDescending(p => p.Metadata != null && p.Metadata.ContainsKey("popularity_score") 
                ? JsonSerializer.Deserialize<decimal>(p.Metadata["popularity_score"].ToString() ?? "0") 
                : 0)
            .Take(limit)
            .Select(p => new ProductSummaryResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                IsAvailable = p.IsAvailable,
                ImageUrl = p.ImageUrl,
                PreparationTimeMinutes = p.PreparationTimeMinutes,
                HasAddOns = p.HasAddOns,
                AddOnsCount = p.AddOns.Count,
                Category = new CategorySummaryResponse
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name,
                    Description = p.Category.Description,
                    ImageUrl = p.Category.ImageUrl,
                    Color = p.Category.Color,
                    Icon = p.Category.Icon,
                    IsActive = p.Category.IsActive,
                    DisplayOrder = p.Category.DisplayOrder,
                    ProductsCount = p.Category.Products.Count
                },
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    private async Task<List<ProductSummaryResponse>> GetSimilarProductsForRecentlyViewedAsync(List<Guid> recentlyViewed, int limit)
    {
        // Implementar lógica para encontrar produtos similares aos visualizados recentemente
        return await GetSimilarProductsForCartAsync(recentlyViewed, limit);
    }

    private decimal CalculatePopularityScore(int viewCount, int orderCount, DateTime createdAt)
    {
        // Algoritmo de popularidade baseado em:
        // - Número de visualizações (peso: 0.3)
        // - Número de pedidos (peso: 0.7)
        // - Fator de decaimento temporal
        
        var daysSinceCreation = (DateTime.UtcNow - createdAt).Days;
        var timeDecay = Math.Exp(-daysSinceCreation / 365.0); // Decaimento anual
        
        var viewScore = viewCount * 0.3m;
        var orderScore = orderCount * 10.0m; // Pedidos têm peso maior
        
        var totalScore = (viewScore + orderScore) * (decimal)timeDecay;
        
        return Math.Max(0, totalScore);
    }

    private async Task<object> CollectTrainingDataAsync()
    {
        // Coletar dados para treinamento do modelo
        // - Histórico de pedidos
        // - Visualizações de produtos
        // - Avaliações
        // - Padrões de compra
        
        return new
        {
            Products = await _context.Products.ToListAsync(),
            Categories = await _context.Categories.ToListAsync(),
            // TODO: Adicionar dados de pedidos quando OrderService estiver integrado
        };
    }

    private async Task CalculateProductSimilaritiesAsync(object trainingData)
    {
        // Implementar cálculo de similaridade entre produtos
        // - Baseado em características (categoria, preço, adicionais)
        // - Baseado em padrões de compra
        // - Collaborative filtering
        
        _logger.LogInformation("Calculando similaridades entre produtos");
        
        // TODO: Implementar algoritmo de similaridade
    }

    private async Task UpdateAllPopularityScoresAsync()
    {
        var products = await _context.Products.ToListAsync();
        
        foreach (var product in products)
        {
            var metadata = product.Metadata ?? new Dictionary<string, object>();
            var viewCount = metadata.ContainsKey("view_count") 
                ? JsonSerializer.Deserialize<int>(metadata["view_count"].ToString() ?? "0") 
                : 0;
            var orderCount = metadata.ContainsKey("total_orders") 
                ? JsonSerializer.Deserialize<int>(metadata["total_orders"].ToString() ?? "0") 
                : 0;
            
            var popularityScore = CalculatePopularityScore(viewCount, orderCount, product.CreatedAt);
            
            metadata["popularity_score"] = popularityScore;
            metadata["last_updated"] = DateTime.UtcNow;
            
            product.Metadata = metadata;
            product.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
    }

    private async Task SaveTrainedModelAsync()
    {
        // Salvar modelo treinado para uso posterior
        // - Serializar similaridades calculadas
        // - Salvar parâmetros do modelo
        // - Salvar métricas de performance
        
        _logger.LogInformation("Modelo treinado salvo com sucesso");
    }

    #endregion
} 