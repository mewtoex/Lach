using System.ComponentModel.DataAnnotations;

namespace Lach.Shared.Common.Models;

public enum OrderStatus
{
    Pending,
    Accepted,
    InProduction,
    ReadyForDelivery,
    OutForDelivery,
    Delivered,
    Cancelled
}

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    
    public List<OrderItem> Items { get; set; } = new();
    
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Total { get; set; }
    
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    public string DeliveryAddress { get; set; } = string.Empty;
    public string? DeliveryInstructions { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? InProductionAt { get; set; }
    public DateTime? ReadyForDeliveryAt { get; set; }
    public DateTime? OutForDeliveryAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string? SpecialInstructions { get; set; }
}

public class CreateOrderRequest
{
    [Required]
    public List<OrderItemRequest> Items { get; set; } = new();
    
    [Required]
    [StringLength(500)]
    public string DeliveryAddress { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? DeliveryInstructions { get; set; }
}

public class OrderItemRequest
{
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
    
    [StringLength(200)]
    public string? SpecialInstructions { get; set; }
}

public class UpdateOrderStatusRequest
{
    [Required]
    public OrderStatus Status { get; set; }
    
    public string? Notes { get; set; }
} 