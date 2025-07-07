using System.ComponentModel.DataAnnotations;

namespace Lach.Shared.Common.Models;

public class Product
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
    
    public bool IsAvailable { get; set; } = true;
    public bool IsSpecial { get; set; } = false;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProductCreateRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
    
    public bool IsSpecial { get; set; } = false;
}

public class ProductUpdateRequest
{
    [StringLength(100)]
    public string? Name { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public decimal? Price { get; set; }
    
    [StringLength(50)]
    public string? Category { get; set; }
    
    [StringLength(500)]
    public string? ImageUrl { get; set; }
    
    public bool? IsAvailable { get; set; }
    public bool? IsSpecial { get; set; }
} 