namespace ProductService.Models;

public record AddOnCategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public string? Color { get; init; }
    public string? Icon { get; init; }
    public bool IsActive { get; init; }
    public int DisplayOrder { get; init; }
    public int MaxSelections { get; init; }
    public bool IsRequired { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public List<ProductAddOnDto> AddOns { get; init; } = new();
}

public record CreateAddOnCategoryDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public string? Color { get; init; }
    public string? Icon { get; init; }
    public bool IsActive { get; init; } = true;
    public int DisplayOrder { get; init; } = 0;
    public int MaxSelections { get; init; } = 3;
    public bool IsRequired { get; init; } = false;
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
}

public record UpdateAddOnCategoryDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public string? Color { get; init; }
    public string? Icon { get; init; }
    public bool? IsActive { get; init; }
    public int? DisplayOrder { get; init; }
    public int? MaxSelections { get; init; }
    public bool? IsRequired { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
}

public record ProductAddOnCategoryDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public Guid AddOnCategoryId { get; init; }
    public int MaxSelections { get; init; }
    public bool IsRequired { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public AddOnCategoryDto AddOnCategory { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CreateProductAddOnCategoryDto
{
    public Guid ProductId { get; init; }
    public Guid AddOnCategoryId { get; init; }
    public int? MaxSelections { get; init; }
    public bool? IsRequired { get; init; }
    public int? DisplayOrder { get; init; }
    public bool IsActive { get; init; } = true;
}

public record UpdateProductAddOnCategoryDto
{
    public int? MaxSelections { get; init; }
    public bool? IsRequired { get; init; }
    public int? DisplayOrder { get; init; }
    public bool? IsActive { get; init; }
} 