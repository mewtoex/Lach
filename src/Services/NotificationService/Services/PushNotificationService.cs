using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace NotificationService.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _serverKey;
    private readonly string _baseUrl;

    public PushNotificationService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _serverKey = _configuration["Firebase:ServerKey"] ?? string.Empty;
        _baseUrl = _configuration["Firebase:BaseUrl"] ?? "https://fcm.googleapis.com/fcm/send";
    }

    public async Task<bool> SendPushNotificationAsync(string userId, string title, string message)
    {
        if (string.IsNullOrEmpty(_serverKey))
        {
            // Log that Firebase is not configured
            return false;
        }

        try
        {
            var payload = new
            {
                to = $"/topics/user_{userId}",
                notification = new
                {
                    title = title,
                    body = message,
                    sound = "default"
                },
                data = new
                {
                    title = title,
                    message = message,
                    click_action = "FLUTTER_NOTIFICATION_CLICK"
                }
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("key", _serverKey);

            var response = await _httpClient.PostAsync(_baseUrl, content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SendPushNotificationToTopicAsync(string topic, string title, string message)
    {
        if (string.IsNullOrEmpty(_serverKey))
        {
            return false;
        }

        try
        {
            var payload = new
            {
                to = $"/topics/{topic}",
                notification = new
                {
                    title = title,
                    body = message,
                    sound = "default"
                },
                data = new
                {
                    title = title,
                    message = message,
                    click_action = "FLUTTER_NOTIFICATION_CLICK"
                }
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("key", _serverKey);

            var response = await _httpClient.PostAsync(_baseUrl, content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SubscribeToTopicAsync(string userId, string topic)
    {
        if (string.IsNullOrEmpty(_serverKey))
        {
            return false;
        }

        try
        {
            // This would require the user's FCM token
            // For now, we'll return true as a placeholder
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UnsubscribeFromTopicAsync(string userId, string topic)
    {
        if (string.IsNullOrEmpty(_serverKey))
        {
            return false;
        }

        try
        {
            // This would require the user's FCM token
            // For now, we'll return true as a placeholder
            return true;
        }
        catch
        {
            return false;
        }
    }
} 