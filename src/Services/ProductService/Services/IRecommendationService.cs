using ProductService.Models;

namespace ProductService.Services;

public interface IRecommendationService
{
    Task<ProductRecommendationResponse> GetRecommendationsAsync(ProductRecommendationRequest request);
    Task<List<ProductSummaryResponse>> GetPopularProductsAsync(int limit = 10);
    Task<List<ProductSummaryResponse>> GetTrendingProductsAsync(int limit = 10);
    Task<List<ProductSummaryResponse>> GetSimilarProductsAsync(Guid productId, int limit = 5);
    Task<List<ProductSummaryResponse>> GetPersonalizedRecommendationsAsync(Guid customerId, int limit = 10);
    Task<List<CategorySummaryResponse>> GetRecommendedCategoriesAsync(Guid customerId, int limit = 5);
    Task UpdateProductPopularityAsync(Guid productId, int viewCount = 1, int orderCount = 0);
    Task TrainRecommendationModelAsync();
    Task<Dictionary<string, object>> GetRecommendationMetricsAsync();
} 