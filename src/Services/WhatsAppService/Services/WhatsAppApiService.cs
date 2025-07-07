using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace WhatsAppService.Services;

public class WhatsAppApiService : IWhatsAppApiService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _phoneNumberId;
    private readonly string _baseUrl;

    public WhatsAppApiService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _apiKey = _configuration["WhatsApp:ApiKey"] ?? string.Empty;
        _phoneNumberId = _configuration["WhatsApp:PhoneNumberId"] ?? string.Empty;
        _baseUrl = _configuration["WhatsApp:BaseUrl"] ?? "https://graph.facebook.com/v18.0";
    }

    public async Task<bool> SendMessageAsync(string phoneNumber, string message)
    {
        if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_phoneNumberId))
        {
            // Log that WhatsApp API is not configured
            return false;
        }

        try
        {
            var url = $"{_baseUrl}/{_phoneNumberId}/messages";
            var payload = new
            {
                messaging_product = "whatsapp",
                to = phoneNumber,
                type = "text",
                text = new { body = message }
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SendTemplateMessageAsync(string phoneNumber, string templateName, Dictionary<string, string> parameters)
    {
        if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_phoneNumberId))
        {
            return false;
        }

        try
        {
            var url = $"{_baseUrl}/{_phoneNumberId}/messages";
            var payload = new
            {
                messaging_product = "whatsapp",
                to = phoneNumber,
                type = "template",
                template = new
                {
                    name = templateName,
                    language = new { code = "pt_BR" },
                    components = new[]
                    {
                        new
                        {
                            type = "body",
                            parameters = parameters.Select(p => new
                            {
                                type = "text",
                                text = p.Value
                            }).ToArray()
                        }
                    }
                }
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<WhatsAppWebhookEvent?> ProcessWebhookAsync(string payload)
    {
        try
        {
            var webhookData = JsonConvert.DeserializeObject<WhatsAppWebhookPayload>(payload);
            
            if (webhookData?.Entry?.Any() == true)
            {
                var entry = webhookData.Entry[0];
                if (entry.Changes?.Any() == true)
                {
                    var change = entry.Changes[0];
                    if (change.Value?.Messages?.Any() == true)
                    {
                        var message = change.Value.Messages[0];
                        return new WhatsAppWebhookEvent
                        {
                            PhoneNumber = message.From,
                            Message = message.Text?.Body ?? string.Empty,
                            MessageType = MessageType.Text, // Simplified for this implementation
                            Timestamp = DateTimeOffset.FromUnixTimeSeconds(message.Timestamp).DateTime
                        };
                    }
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}

// WhatsApp Webhook Response Models
public class WhatsAppWebhookPayload
{
    public string Object { get; set; } = string.Empty;
    public List<WebhookEntry> Entry { get; set; } = new();
}

public class WebhookEntry
{
    public string Id { get; set; } = string.Empty;
    public List<WebhookChange> Changes { get; set; } = new();
}

public class WebhookChange
{
    public string Field { get; set; } = string.Empty;
    public WebhookValue Value { get; set; } = new();
}

public class WebhookValue
{
    public string MessagingProduct { get; set; } = string.Empty;
    public List<WebhookMessage> Messages { get; set; } = new();
}

public class WebhookMessage
{
    public string From { get; set; } = string.Empty;
    public long Timestamp { get; set; }
    public WebhookText? Text { get; set; }
}

public class WebhookText
{
    public string Body { get; set; } = string.Empty;
} 