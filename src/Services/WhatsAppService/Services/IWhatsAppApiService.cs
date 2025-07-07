namespace WhatsAppService.Services;

public interface IWhatsAppApiService
{
    Task<bool> SendMessageAsync(string phoneNumber, string message);
    Task<bool> SendTemplateMessageAsync(string phoneNumber, string templateName, Dictionary<string, string> parameters);
    Task<WhatsAppWebhookEvent?> ProcessWebhookAsync(string payload);
}

public class WhatsAppWebhookEvent
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public MessageType MessageType { get; set; }
    public DateTime Timestamp { get; set; }
} 