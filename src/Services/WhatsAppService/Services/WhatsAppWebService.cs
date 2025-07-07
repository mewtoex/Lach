using System.Text.Json;
using Microsoft.Extensions.Options;

namespace WhatsAppService.Services;

public class WhatsAppWebService : IWhatsAppWebService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WhatsAppWebService> _logger;
    private readonly WhatsAppWebOptions _options;

    public WhatsAppWebService(
        HttpClient httpClient,
        ILogger<WhatsAppWebService> logger,
        IOptions<WhatsAppWebOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<bool> InitializeAsync(string sessionId)
    {
        try
        {
            var request = new
            {
                sessionId = sessionId,
                headless = true,
                puppeteerArgs = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            };

            var response = await _httpClient.PostAsync($"{_options.WebServiceUrl}/initialize", 
                new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("WhatsApp Web initialized for session: {SessionId}", sessionId);
                return true;
            }

            _logger.LogError("Failed to initialize WhatsApp Web for session: {SessionId}", sessionId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing WhatsApp Web for session: {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<string?> GetQrCodeAsync(string sessionId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_options.WebServiceUrl}/qr/{sessionId}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<QrCodeResponse>(content);
                return result?.QrCode;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting QR code for session: {SessionId}", sessionId);
            return null;
        }
    }

    public async Task<bool> IsConnectedAsync(string sessionId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_options.WebServiceUrl}/status/{sessionId}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<StatusResponse>(content);
                return result?.Connected ?? false;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking connection status for session: {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<bool> SendMessageAsync(string toNumber, string content)
    {
        try
        {
            var request = new
            {
                to = toNumber,
                content = content
            };

            var response = await _httpClient.PostAsync($"{_options.WebServiceUrl}/send", 
                new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Message sent successfully to: {ToNumber}", toNumber);
                return true;
            }

            _logger.LogError("Failed to send message to: {ToNumber}", toNumber);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to: {ToNumber}", toNumber);
            return false;
        }
    }

    public async Task<bool> DisconnectAsync(string sessionId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{_options.WebServiceUrl}/session/{sessionId}");
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("WhatsApp Web disconnected for session: {SessionId}", sessionId);
                return true;
            }

            _logger.LogError("Failed to disconnect WhatsApp Web for session: {SessionId}", sessionId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting WhatsApp Web for session: {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<Dictionary<string, object>> GetSessionInfoAsync(string sessionId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_options.WebServiceUrl}/info/{sessionId}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                return result ?? new Dictionary<string, object>();
            }

            return new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session info for session: {SessionId}", sessionId);
            return new Dictionary<string, object>();
        }
    }

    private class QrCodeResponse
    {
        public string? QrCode { get; set; }
    }

    private class StatusResponse
    {
        public bool Connected { get; set; }
    }
}

public class WhatsAppWebOptions
{
    public string WebServiceUrl { get; set; } = "http://localhost:3003";
    public int TimeoutSeconds { get; set; } = 30;
} 