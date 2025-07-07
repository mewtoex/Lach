using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using Serilog;
using Serilog.Events;

namespace Lach.Shared.Monitoring.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMonitoring(this IServiceCollection services, IHostBuilder hostBuilder)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "Lach-System")
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File("logs/lach-.log", 
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.Seq("http://seq:5341")
            .CreateLogger();

        hostBuilder.UseSerilog();

        // Add Prometheus metrics
        services.AddSingleton<IMetricsService, MetricsService>();

        return services;
    }

    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddNpgSql(configuration.GetConnectionString("PostgreSQL") ?? "Host=postgres;Database=lach_db;Username=lach_user;Password=lach_password")
            .AddRabbitMQ(configuration.GetConnectionString("RabbitMQ") ?? "amqp://guest:guest@rabbitmq:5672/")
            .ForwardToPrometheus();

        return services;
    }
}

public interface IMetricsService
{
    Counter RequestCounter { get; }
    Histogram RequestDuration { get; }
    Counter ErrorCounter { get; }
    Gauge ActiveConnections { get; }
    Counter OrderCreatedCounter { get; }
    Counter OrderStatusChangedCounter { get; }
    Histogram OrderProcessingDuration { get; }
}

public class MetricsService : IMetricsService
{
    public Counter RequestCounter { get; }
    public Histogram RequestDuration { get; }
    public Counter ErrorCounter { get; }
    public Gauge ActiveConnections { get; }
    public Counter OrderCreatedCounter { get; }
    public Counter OrderStatusChangedCounter { get; }
    public Histogram OrderProcessingDuration { get; }

    public MetricsService()
    {
        RequestCounter = Metrics.CreateCounter("http_requests_total", "Total HTTP requests", new CounterConfiguration
        {
            LabelNames = new[] { "method", "endpoint", "status_code" }
        });

        RequestDuration = Metrics.CreateHistogram("http_request_duration_seconds", "HTTP request duration", new HistogramConfiguration
        {
            LabelNames = new[] { "method", "endpoint" },
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 10)
        });

        ErrorCounter = Metrics.CreateCounter("errors_total", "Total errors", new CounterConfiguration
        {
            LabelNames = new[] { "service", "error_type" }
        });

        ActiveConnections = Metrics.CreateGauge("active_connections", "Active database connections", new GaugeConfiguration
        {
            LabelNames = new[] { "service" }
        });

        OrderCreatedCounter = Metrics.CreateCounter("orders_created_total", "Total orders created", new CounterConfiguration
        {
            LabelNames = new[] { "service" }
        });

        OrderStatusChangedCounter = Metrics.CreateCounter("order_status_changes_total", "Total order status changes", new CounterConfiguration
        {
            LabelNames = new[] { "service", "from_status", "to_status" }
        });

        OrderProcessingDuration = Metrics.CreateHistogram("order_processing_duration_seconds", "Order processing duration", new HistogramConfiguration
        {
            LabelNames = new[] { "service" },
            Buckets = Histogram.ExponentialBuckets(0.1, 2, 10)
        });
    }
} 