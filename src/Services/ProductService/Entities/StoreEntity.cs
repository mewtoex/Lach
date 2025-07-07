using System.ComponentModel.DataAnnotations;

namespace ProductService.Entities;

public class StoreEntity
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(18)]
    public string Cnpj { get; set; } = string.Empty;
    
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
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [MaxLength(100)]
    public string? Email { get; set; }
    
    [MaxLength(200)]
    public string? Website { get; set; }
    
    [MaxLength(200)]
    public string? LogoUrl { get; set; }
    
    [MaxLength(200)]
    public string? CoverImageUrl { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public TimeSpan OpeningTime { get; set; } = new TimeSpan(8, 0, 0);
    
    public TimeSpan ClosingTime { get; set; } = new TimeSpan(22, 0, 0);
    
    public bool IsOpenOnWeekends { get; set; } = true;
    
    public bool IsOpenOnHolidays { get; set; } = false;
    
    // Configurações de entrega
    public decimal DeliveryBasePrice { get; set; } = 5.00m; // Taxa base
    
    public decimal DeliveryPricePerKm { get; set; } = 2.00m; // Taxa por KM
    
    public decimal FreeDeliveryThreshold { get; set; } = 30.00m; // Pedido mínimo para entrega grátis
    
    public decimal MaxDeliveryDistance { get; set; } = 10.0m; // Distância máxima em KM
    
    public int EstimatedDeliveryTimeMinutes { get; set; } = 45; // Tempo estimado de entrega
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
} 