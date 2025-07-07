namespace OrderService.Models;

#region Order DTOs

public class CreateOrderRequest
{
    public Guid CustomerId { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
    public DeliveryAddressRequest DeliveryAddress { get; set; } = new();
    public string PaymentMethod { get; set; } = string.Empty;
    public string? DeliveryInstructions { get; set; }
    public string? CouponCode { get; set; }
}

public class UpdateOrderRequest
{
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public DateTime? EstimatedDeliveryTime { get; set; }
    public string? DeliveryPersonId { get; set; }
}

public class OrderResponse
{
    public string Id { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
    public DeliveryAddressResponse DeliveryAddress { get; set; } = new();
    public string PaymentMethod { get; set; } = string.Empty;
    public string? DeliveryInstructions { get; set; }
    public string? CouponCode { get; set; }
    public DateTime? EstimatedDeliveryTime { get; set; }
    public DateTime? ActualDeliveryTime { get; set; }
    public string? DeliveryPersonId { get; set; }
    public string? DeliveryPersonName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class OrderSummaryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public DateTime? EstimatedDeliveryTime { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderStatusUpdateRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? EstimatedDeliveryTime { get; set; }
}

public class OrderFilterRequest
{
    public string? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? CustomerId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class OrderStatisticsResponse
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int InProductionOrders { get; set; }
    public int ReadyOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<DailyOrderStats> DailyStats { get; set; } = new();
}

public class DailyOrderStats
{
    public DateTime Date { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
}

#endregion

#region Order Item DTOs

public class OrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? SpecialInstructions { get; set; }
    public List<OrderItemAddOnRequest>? AddOns { get; set; }
}

public class OrderItemAddOnRequest
{
    public Guid AddOnId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class OrderItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductDescription { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? SpecialInstructions { get; set; }
    public List<OrderItemAddOnResponse> AddOns { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class OrderItemAddOnResponse
{
    public Guid Id { get; set; }
    public Guid AddOnId { get; set; }
    public string AddOnName { get; set; } = string.Empty;
    public string AddOnCategory { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

#endregion

#region Delivery DTOs

public class DeliveryAddressRequest
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string? Complement { get; set; }
    public string? Reference { get; set; }
}

public class DeliveryAddressResponse
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string? Complement { get; set; }
    public string? Reference { get; set; }
    public string FullAddress { get; set; } = string.Empty;
}

public class DeliveryAssignmentRequest
{
    public string OrderId { get; set; } = string.Empty;
    public string DeliveryPersonId { get; set; } = string.Empty;
    public DateTime? EstimatedPickupTime { get; set; }
}

public class DeliveryStatusUpdateRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? ActualTime { get; set; }
    public string? Location { get; set; }
}

#endregion 