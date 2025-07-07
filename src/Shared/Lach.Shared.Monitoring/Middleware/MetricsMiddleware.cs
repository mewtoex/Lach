using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using Lach.Shared.Monitoring.Extensions;

namespace Lach.Shared.Monitoring.Middleware;

public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMetricsService _metricsService;
    private readonly ILogger<MetricsMiddleware> _logger;

    public MetricsMiddleware(RequestDelegate next, IMetricsService metricsService, ILogger<MetricsMiddleware> logger)
    {
        _next = next;
        _metricsService = metricsService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            stopwatch.Stop();

            // Record metrics
            var method = context.Request.Method;
            var endpoint = context.Request.Path.Value ?? "/";
            var statusCode = context.Response.StatusCode.ToString();

            _metricsService.RequestCounter.WithLabels(method, endpoint, statusCode).Inc();
            _metricsService.RequestDuration.WithLabels(method, endpoint).Observe(stopwatch.Elapsed.TotalSeconds);

            // Log request
            _logger.LogInformation("HTTP {Method} {Endpoint} {StatusCode} - {Duration}ms", 
                method, endpoint, statusCode, stopwatch.ElapsedMilliseconds);

            // Copy response back to original stream
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Record error metrics
            var method = context.Request.Method;
            var endpoint = context.Request.Path.Value ?? "/";
            var errorType = ex.GetType().Name;

            _metricsService.ErrorCounter.WithLabels("api", errorType).Inc();
            _metricsService.RequestCounter.WithLabels(method, endpoint, "500").Inc();

            // Log error
            _logger.LogError(ex, "HTTP {Method} {Endpoint} failed after {Duration}ms", 
                method, endpoint, stopwatch.ElapsedMilliseconds);

            // Restore original stream and return error
            context.Response.Body = originalBodyStream;
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal Server Error");
        }
    }
}

public static class MetricsMiddlewareExtensions
{
    public static IApplicationBuilder UseMetrics(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MetricsMiddleware>();
    }
} 