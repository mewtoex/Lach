namespace NotificationService.Entities;

public enum NotificationType
{
    OrderCreated,
    OrderStatusChanged,
    OrderDelivered,
    OrderCancelled,
    PaymentReceived,
    PaymentFailed,
    SystemAlert,
    Promotional
}

public enum NotificationStatus
{
    Pending,
    Sent,
    Delivered,
    Read,
    Failed
}

public enum NotificationChannel
{
    Email,
    SMS,
    Push,
    WhatsApp,
    InApp
}

public class NotificationEntity
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty; // Email, phone, or user ID
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public NotificationChannel Channel { get; set; }
    public string? Metadata { get; set; } // JSON for additional data
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
} 