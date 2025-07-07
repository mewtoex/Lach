using System.ComponentModel.DataAnnotations;

namespace ProductService.Entities;

public class AddOnCategoryEntity
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(200)]
    public string? ImageUrl { get; set; }
    
    [MaxLength(50)]
    public string? Color { get; set; } // Para UI
    
    [MaxLength(50)]
    public string? Icon { get; set; } // Para UI
    
    public bool IsActive { get; set; } = true;
    
    public int DisplayOrder { get; set; } = 0;
    
    public int MaxSelections { get; set; } = 3; // Máximo de adicionais que podem ser selecionados desta categoria
    
    public bool IsRequired { get; set; } = false; // Se é obrigatório selecionar pelo menos um
    
    public decimal? MinPrice { get; set; } // Preço mínimo para adicionais desta categoria
    
    public decimal? MaxPrice { get; set; } // Preço máximo para adicionais desta categoria
    
    // Relacionamento com adicionais
    public List<ProductAddOnEntity> AddOns { get; set; } = new();
    
    // Relacionamento com produtos (quais produtos podem ter esta categoria de adicional)
    public List<ProductAddOnCategoryEntity> ProductAddOnCategories { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
} 