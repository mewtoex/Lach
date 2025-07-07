using System.ComponentModel.DataAnnotations;

namespace ProductService.Entities;

public class ProductAddOnEntity
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public decimal Price { get; set; }
    
    [MaxLength(200)]
    public string? ImageUrl { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    
    public int MaxQuantity { get; set; } = 5; // Quantidade máxima que pode ser adicionada
    
    // Relacionamento com categoria de adicional
    public Guid AddOnCategoryId { get; set; }
    public AddOnCategoryEntity AddOnCategory { get; set; } = null!;
    
    // Relacionamento com produto (opcional - pode ser global)
    public Guid? ProductId { get; set; }
    public ProductEntity? Product { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
} 