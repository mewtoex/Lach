using System.ComponentModel.DataAnnotations;

namespace OrderService.Entities;

public class OrderItemEntity
{
    public Guid Id { get; set; }
    
    public Guid OrderId { get; set; }
    public OrderEntity Order { get; set; } = null!;
    
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductDescription { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    
    [MaxLength(500)]
    public string? SpecialInstructions { get; set; }
    
    public List<OrderItemAddOnEntity> AddOns { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
} 