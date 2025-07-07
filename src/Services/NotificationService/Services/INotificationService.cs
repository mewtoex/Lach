using Lach.Shared.Common.Models;

namespace NotificationService.Services;

public interface INotificationService
{
    Task<Notification> SendNotificationAsync(NotificationRequest request);
    Task<Notification> SendOrderStatusNotificationAsync(Guid orderId, OrderStatus status, string recipient);
    Task<Notification> SendOrderCreatedNotificationAsync(Order order);
    Task<Notification> SendPaymentNotificationAsync(Guid orderId, bool isSuccess, string recipient);
    Task<List<Notification>> GetNotificationsByRecipientAsync(string recipient);
    Task<Notification> MarkAsReadAsync(Guid notificationId);
    Task<Notification> MarkAsDeliveredAsync(Guid notificationId);
    Task<List<Notification>> GetPendingNotificationsAsync();
    Task<bool> DeleteNotificationAsync(Guid notificationId);
}

public class Notification
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; }
    public NotificationChannel Channel { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class NotificationRequest
{
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
} 