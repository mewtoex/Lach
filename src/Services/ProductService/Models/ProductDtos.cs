namespace ProductService.Models;

#region Product DTOs

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public int PreparationTimeMinutes { get; set; } = 15;
    public bool HasAddOns { get; set; } = false;
    public List<CreateProductAddOnRequest>? AddOns { get; set; }
    public Dictionary<string, object>? Metadata { get; set; } // Para ML features
}

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public Guid? CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public int? PreparationTimeMinutes { get; set; }
    public bool? HasAddOns { get; set; }
    public bool? IsAvailable { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public int PreparationTimeMinutes { get; set; }
    public bool HasAddOns { get; set; }
    public CategorySummaryResponse Category { get; set; } = new();
    public List<ProductAddOnResponse> AddOns { get; set; } = new();
    public Dictionary<string, object>? Metadata { get; set; }
    public ProductStats Stats { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProductSummaryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public int PreparationTimeMinutes { get; set; }
    public bool HasAddOns { get; set; }
    public int AddOnsCount { get; set; }
    public CategorySummaryResponse Category { get; set; } = new();
    public ProductStats Stats { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class ProductStats
{
    public int TotalOrders { get; set; }
    public int MonthlyOrders { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public decimal PopularityScore { get; set; }
    public List<string> FrequentlyOrderedWith { get; set; } = new();
}

public class ProductFilterRequest
{
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsAvailable { get; set; }
    public bool? HasAddOns { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MaxPreparationTime { get; set; }
    public string? SortBy { get; set; } // name, price, popularity, rating
    public string? SortOrder { get; set; } // asc, desc
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ProductRecommendationRequest
{
    public Guid? CustomerId { get; set; }
    public List<Guid>? RecentlyViewedProducts { get; set; }
    public List<Guid>? CartItems { get; set; }
    public string? UserPreferences { get; set; }
    public Guid? CategoryId { get; set; }
    public int Limit { get; set; } = 10;
}

public class ProductRecommendationResponse
{
    public List<ProductSummaryResponse> RecommendedProducts { get; set; } = new();
    public string RecommendationReason { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
}

#endregion

#region Product AddOn DTOs

public class CreateProductAddOnRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty; // Frutas, Granola, Adoçantes, etc.
    public string? ImageUrl { get; set; }
    public int MaxQuantity { get; set; } = 5;
    public bool IsAvailable { get; set; } = true;
}

public class UpdateProductAddOnRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }
    public int? MaxQuantity { get; set; }
    public bool? IsAvailable { get; set; }
}

public class ProductAddOnResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public int MaxQuantity { get; set; }
    public Guid ProductId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProductAddOnSummaryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public int MaxQuantity { get; set; }
}

#endregion

#region Product Analytics DTOs

public class ProductAnalyticsRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? GroupBy { get; set; } // day, week, month
}

public class ProductAnalyticsResponse
{
    public List<ProductAnalyticsData> Data { get; set; } = new();
    public ProductAnalyticsSummary Summary { get; set; } = new();
}

public class ProductAnalyticsData
{
    public DateTime Date { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageRating { get; set; }
    public int ViewCount { get; set; }
}

public class ProductAnalyticsSummary
{
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<TopProduct> TopProducts { get; set; } = new();
    public List<TopAddOn> TopAddOns { get; set; } = new();
    public List<TopCategory> TopCategories { get; set; } = new();
}

public class TopProduct
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal PopularityScore { get; set; }
}

public class TopAddOn
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
}

public class TopCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProductsCount { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
}

#endregion

public record ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public bool IsAvailable { get; init; }
    public string? ImageUrl { get; init; }
    public int PreparationTimeMinutes { get; init; }
    public bool HasAddOns { get; init; }
    public Guid CategoryId { get; init; }
    public CategoryDto Category { get; init; } = null!;
    public List<ProductAddOnCategoryDto> AddOnCategories { get; init; } = new();
    public List<ProductAddOnDto> SpecificAddOns { get; init; } = new();
    public Dictionary<string, object>? Metadata { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CreateProductDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public bool IsAvailable { get; init; } = true;
    public string? ImageUrl { get; init; }
    public int PreparationTimeMinutes { get; init; } = 15;
    public bool HasAddOns { get; init; } = false;
    public Guid CategoryId { get; init; }
    public List<Guid> AddOnCategoryIds { get; init; } = new();
    public Dictionary<string, object>? Metadata { get; init; }
}

public record UpdateProductDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public decimal? Price { get; init; }
    public bool? IsAvailable { get; init; }
    public string? ImageUrl { get; init; }
    public int? PreparationTimeMinutes { get; init; }
    public bool? HasAddOns { get; init; }
    public Guid? CategoryId { get; init; }
    public List<Guid>? AddOnCategoryIds { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

public record ProductAddOnDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string? ImageUrl { get; init; }
    public bool IsAvailable { get; init; }
    public int MaxQuantity { get; init; }
    public Guid AddOnCategoryId { get; init; }
    public AddOnCategoryDto AddOnCategory { get; init; } = null!;
    public Guid? ProductId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CreateProductAddOnDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string? ImageUrl { get; init; }
    public bool IsAvailable { get; init; } = true;
    public int MaxQuantity { get; init; } = 5;
    public Guid AddOnCategoryId { get; init; }
    public Guid? ProductId { get; init; }
}

public record UpdateProductAddOnDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public decimal? Price { get; init; }
    public string? ImageUrl { get; init; }
    public bool? IsAvailable { get; init; }
    public int? MaxQuantity { get; init; }
    public Guid? AddOnCategoryId { get; init; }
    public Guid? ProductId { get; init; }
} 