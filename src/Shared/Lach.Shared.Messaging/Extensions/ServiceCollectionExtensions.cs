using Lach.Shared.Messaging.Configuration;
using Lach.Shared.Messaging.Interfaces;
using Lach.Shared.Messaging.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lach.Shared.Messaging.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMQConfig = RabbitMQConfig.FromConfiguration(configuration);
        
        services.AddSingleton(rabbitMQConfig);
        services.AddSingleton<IMessageBus, RabbitMQMessageBus>();
        
        return services;
    }
} 