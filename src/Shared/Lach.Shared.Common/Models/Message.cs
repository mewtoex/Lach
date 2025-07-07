namespace Lach.Shared.Common.Models;

public abstract class BaseMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string MessageType { get; set; } = string.Empty;
}

// Order Events
public class OrderCreatedEvent : BaseMessage
{
    public Order Order { get; set; } = new();
}

public class OrderStatusUpdatedEvent : BaseMessage
{
    public Guid OrderId { get; set; }
    public OrderStatus PreviousStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public string? Notes { get; set; }
}

public class OrderAcceptedEvent : BaseMessage
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerPhone { get; set; } = string.Empty;
}

public class OrderCancelledEvent : BaseMessage
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerPhone { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

// Production Queue Events
public class OrderAddedToQueueEvent : BaseMessage
{
    public Guid OrderId { get; set; }
    public int Position { get; set; }
}

public class OrderRemovedFromQueueEvent : BaseMessage
{
    public Guid OrderId { get; set; }
}

public class QueueOrderChangedEvent : BaseMessage
{
    public Guid OrderId { get; set; }
    public int NewPosition { get; set; }
}

// Delivery Events
public class DeliveryFeeCalculatedEvent : BaseMessage
{
    public Guid OrderId { get; set; }
    public decimal DeliveryFee { get; set; }
    public double DistanceInKm { get; set; }
}

// WhatsApp Events
public class WhatsAppMessageEvent : BaseMessage
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string MessageType { get; set; } = "text"; // text, image, document
}

public class WhatsAppOrderStatusNotificationEvent : BaseMessage
{
    public string CustomerPhone { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
}

// Notification Events
public class NotificationEvent : BaseMessage
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "info"; // info, success, warning, error
    public Dictionary<string, object>? Data { get; set; }
} 