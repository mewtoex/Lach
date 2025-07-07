using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace NotificationService.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _fromEmail;
    private readonly string _baseUrl;

    public EmailService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _apiKey = _configuration["Email:ApiKey"] ?? string.Empty;
        _fromEmail = _configuration["Email:FromEmail"] ?? "noreply@lanchonete-lach.com";
        _baseUrl = _configuration["Email:BaseUrl"] ?? "https://api.sendgrid.com/v3";
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            // Log that email service is not configured
            return false;
        }

        try
        {
            var payload = new
            {
                personalizations = new[]
                {
                    new
                    {
                        to = new[]
                        {
                            new { email = to }
                        }
                    }
                },
                from = new { email = _fromEmail },
                subject = subject,
                content = new[]
                {
                    new
                    {
                        type = "text/html",
                        value = body
                    }
                }
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Lanchonete-Lach-NotificationService");

            var response = await _httpClient.PostAsync($"{_baseUrl}/mail/send", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SendEmailWithTemplateAsync(string to, string templateName, Dictionary<string, object> parameters)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return false;
        }

        try
        {
            var payload = new
            {
                personalizations = new[]
                {
                    new
                    {
                        to = new[]
                        {
                            new { email = to }
                        },
                        dynamic_template_data = parameters
                    }
                },
                from = new { email = _fromEmail },
                template_id = templateName
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Lanchonete-Lach-NotificationService");

            var response = await _httpClient.PostAsync($"{_baseUrl}/mail/send", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
} 