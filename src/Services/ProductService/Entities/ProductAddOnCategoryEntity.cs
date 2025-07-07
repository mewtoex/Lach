using System.ComponentModel.DataAnnotations;

namespace ProductService.Entities;

public class ProductAddOnCategoryEntity
{
    public Guid Id { get; set; }
    
    // Relacionamento com produto
    public Guid ProductId { get; set; }
    public ProductEntity Product { get; set; } = null!;
    
    // Relacionamento com categoria de adicional
    public Guid AddOnCategoryId { get; set; }
    public AddOnCategoryEntity AddOnCategory { get; set; } = null!;
    
    // Configurações específicas para este produto + categoria
    public int MaxSelections { get; set; } = 3; // Sobrescreve o valor da categoria se necessário
    
    public bool IsRequired { get; set; } = false; // Sobrescreve o valor da categoria se necessário
    
    public int DisplayOrder { get; set; } = 0; // Ordem de exibição específica para este produto
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
} 