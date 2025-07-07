using System.ComponentModel.DataAnnotations;

namespace ProductService.Entities;

public class CustomerAddressEntity
{
    public Guid Id { get; set; }
    
    // Relacionamento com cliente
    public Guid CustomerId { get; set; }
    public CustomerEntity Customer { get; set; } = null!;
    
    [Required]
    [MaxLength(200)]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2)]
    public string State { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(10)]
    public string ZipCode { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Neighborhood { get; set; }
    
    [MaxLength(20)]
    public string? Number { get; set; }
    
    [MaxLength(200)]
    public string? Complement { get; set; }
    
    [Required]
    public decimal Latitude { get; set; }
    
    [Required]
    public decimal Longitude { get; set; }
    
    [MaxLength(100)]
    public string? Reference { get; set; } // Ponto de referência
    
    [MaxLength(100)]
    public string? Label { get; set; } // Casa, Trabalho, etc.
    
    public bool IsDefault { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
} 