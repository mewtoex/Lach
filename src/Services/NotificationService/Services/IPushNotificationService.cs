namespace NotificationService.Services;

public interface IPushNotificationService
{
    Task<bool> SendPushNotificationAsync(string userId, string title, string message);
    Task<bool> SendPushNotificationToTopicAsync(string topic, string title, string message);
    Task<bool> SubscribeToTopicAsync(string userId, string topic);
    Task<bool> UnsubscribeFromTopicAsync(string userId, string topic);
}

public class PushNotificationRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object>? Data { get; set; }
} 