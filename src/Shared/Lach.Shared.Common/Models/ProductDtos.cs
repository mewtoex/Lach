namespace Lach.Shared.Common.Models;

#region Product DTOs

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int PreparationTimeMinutes { get; set; } = 15;
    public bool HasAddOns { get; set; } = false;
    public List<CreateProductAddOnRequest>? AddOns { get; set; }
}

public class UpdateProductRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }
    public int? PreparationTimeMinutes { get; set; }
    public bool? HasAddOns { get; set; }
    public bool? IsAvailable { get; set; }
}

public class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public int PreparationTimeMinutes { get; set; }
    public bool HasAddOns { get; set; }
    public List<ProductAddOnResponse> AddOns { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProductSummaryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public int PreparationTimeMinutes { get; set; }
    public bool HasAddOns { get; set; }
    public int AddOnsCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion

#region Product AddOn DTOs

public class CreateProductAddOnRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int MaxQuantity { get; set; } = 5;
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

#region Order Item DTOs

public class OrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? SpecialInstructions { get; set; }
    public List<OrderItemAddOnRequest>? AddOns { get; set; }
}

public class OrderItemAddOnRequest
{
    public Guid AddOnId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class OrderItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? SpecialInstructions { get; set; }
    public List<OrderItemAddOnResponse> AddOns { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class OrderItemAddOnResponse
{
    public Guid Id { get; set; }
    public Guid AddOnId { get; set; }
    public string AddOnName { get; set; } = string.Empty;
    public string AddOnCategory { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

#endregion 