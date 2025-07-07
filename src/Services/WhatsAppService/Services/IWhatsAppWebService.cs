namespace WhatsAppService.Services;

public interface IWhatsAppWebService
{
    Task<bool> InitializeAsync(string sessionId);
    Task<string?> GetQrCodeAsync(string sessionId);
    Task<bool> IsConnectedAsync(string sessionId);
    Task<bool> SendMessageAsync(string toNumber, string content);
    Task<bool> DisconnectAsync(string sessionId);
    Task<Dictionary<string, object>> GetSessionInfoAsync(string sessionId);
} 