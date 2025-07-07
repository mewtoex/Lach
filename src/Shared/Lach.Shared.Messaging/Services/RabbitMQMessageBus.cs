using System.Text;
using Lach.Shared.Common.Models;
using Lach.Shared.Messaging.Configuration;
using Lach.Shared.Messaging.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Lach.Shared.Messaging.Services;

public class RabbitMQMessageBus : IMessageBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQMessageBus> _logger;
    private readonly Dictionary<string, IModel> _consumerChannels = new();

    public RabbitMQMessageBus(RabbitMQConfig config, ILogger<RabbitMQMessageBus> logger)
    {
        _logger = logger;
        
        var factory = new ConnectionFactory
        {
            HostName = config.Host,
            Port = config.Port,
            UserName = config.Username,
            Password = config.Password,
            VirtualHost = config.VirtualHost
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        // Declare exchange
        _channel.ExchangeDeclare("lach_exchange", ExchangeType.Topic, true, false);
        
        _logger.LogInformation("RabbitMQ connection established");
    }

    public async Task PublishAsync<T>(T message, string routingKey) where T : BaseMessage
    {
        try
        {
            message.MessageType = typeof(T).Name;
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(
                exchange: "lach_exchange",
                routingKey: routingKey,
                basicProperties: null,
                body: body);

            _logger.LogInformation("Message published: {MessageType} with routing key: {RoutingKey}", 
                message.MessageType, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message: {MessageType}", typeof(T).Name);
            throw;
        }
    }

    public async Task SubscribeAsync<T>(string queueName, string routingKey, Func<T, Task> handler) where T : BaseMessage
    {
        try
        {
            var channel = _connection.CreateModel();
            _consumerChannels[queueName] = channel;

            // Declare queue
            channel.QueueDeclare(queueName, true, false, false);
            channel.QueueBind(queueName, "lach_exchange", routingKey);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var deserializedMessage = JsonConvert.DeserializeObject<T>(message);

                    if (deserializedMessage != null)
                    {
                        await handler(deserializedMessage);
                        channel.BasicAck(ea.DeliveryTag, false);
                        
                        _logger.LogInformation("Message processed: {MessageType}", typeof(T).Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message: {MessageType}", typeof(T).Name);
                    channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            channel.BasicConsume(queue: queueName,
                               autoAck: false,
                               consumer: consumer);

            _logger.LogInformation("Subscribed to queue: {QueueName} with routing key: {RoutingKey}", 
                queueName, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to queue: {QueueName}", queueName);
            throw;
        }
    }

    public async Task UnsubscribeAsync(string queueName)
    {
        if (_consumerChannels.TryGetValue(queueName, out var channel))
        {
            channel.Close();
            _consumerChannels.Remove(queueName);
            _logger.LogInformation("Unsubscribed from queue: {QueueName}", queueName);
        }
    }

    public void Dispose()
    {
        foreach (var channel in _consumerChannels.Values)
        {
            channel?.Close();
        }
        _consumerChannels.Clear();
        
        _channel?.Close();
        _connection?.Close();
        
        _logger.LogInformation("RabbitMQ connection disposed");
    }
} 