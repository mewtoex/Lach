using Lach.Shared.Common.Models;

namespace Lach.Shared.Messaging.Interfaces;

public interface IMessageBus
{
    Task PublishAsync<T>(T message, string routingKey) where T : BaseMessage;
    Task SubscribeAsync<T>(string queueName, string routingKey, Func<T, Task> handler) where T : BaseMessage;
    Task UnsubscribeAsync(string queueName);
    void Dispose();
} 