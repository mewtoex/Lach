using System.ComponentModel.DataAnnotations;

namespace ProductService.Entities;

public class ProductEntity
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public decimal Price { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    
    [MaxLength(200)]
    public string? ImageUrl { get; set; }
    
    public int PreparationTimeMinutes { get; set; } = 15;
    
    public bool HasAddOns { get; set; } = false;
    
    // Relacionamento com categoria
    public Guid CategoryId { get; set; }
    public CategoryEntity Category { get; set; } = null!;
    
    // Relacionamento com categorias de adicionais
    public List<ProductAddOnCategoryEntity> ProductAddOnCategories { get; set; } = new();
    
    // Relacionamento direto com adicionais (para adicionais específicos do produto)
    public List<ProductAddOnEntity> SpecificAddOns { get; set; } = new();
    
    // Campos para ML
    public Dictionary<string, object>? Metadata { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
} 