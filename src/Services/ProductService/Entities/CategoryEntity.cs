using System.ComponentModel.DataAnnotations;

namespace ProductService.Entities;

public class CategoryEntity
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(200)]
    public string? ImageUrl { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int DisplayOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Color { get; set; } // Para UI
    
    [MaxLength(50)]
    public string? Icon { get; set; } // Para UI
    
    // Relacionamento com produtos
    public List<ProductEntity> Products { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
} 