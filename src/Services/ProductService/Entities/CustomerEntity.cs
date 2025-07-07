using System.ComponentModel.DataAnnotations;

namespace ProductService.Entities;

public class CustomerEntity
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;
    
    [MaxLength(14)]
    public string? Cpf { get; set; }
    
    [MaxLength(200)]
    public string? ProfileImageUrl { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    [MaxLength(10)]
    public string? Gender { get; set; } // M, F, O
    
    public bool IsActive { get; set; } = true;
    
    public bool IsVerified { get; set; } = false;
    
    public DateTime? LastLoginAt { get; set; }
    
    // Endereços do cliente
    public List<CustomerAddressEntity> Addresses { get; set; } = new();
    
    // Endereço padrão
    public Guid? DefaultAddressId { get; set; }
    public CustomerAddressEntity? DefaultAddress { get; set; }
    
    // Preferências
    public Dictionary<string, object>? Preferences { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
} 