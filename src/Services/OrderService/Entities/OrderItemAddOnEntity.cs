using System.ComponentModel.DataAnnotations;

namespace OrderService.Entities;

public class OrderItemAddOnEntity
{
    public Guid Id { get; set; }
    
    public Guid OrderItemId { get; set; }
    public OrderItemEntity OrderItem { get; set; } = null!;
    
    public Guid AddOnId { get; set; }
    public string AddOnName { get; set; } = string.Empty;
    public string AddOnCategory { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
} 