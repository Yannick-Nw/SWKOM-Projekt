using Domain.Messaging;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Messaging;

public sealed class RabbitMqMessageQueueService : IMessageQueueService
{
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly ILogger _logger;

    public RabbitMqMessageQueueService(IConnectionFactory connectionFactory, ILogger<RabbitMqMessageQueueService> logger)
    {
        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
        _logger = logger;
    }

    public void Publish<T>(T message) where T : IMessage
    {
        _channel.QueueDeclare(queue: T.Channel, durable: true, exclusive: false, autoDelete: false, arguments: null);

        // Convert message to json byte array
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        _channel.BasicPublish(exchange: string.Empty, routingKey: T.Channel, basicProperties: null, body: body);
        _logger.LogInformation("Message published: {message}", message);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();

        GC.SuppressFinalize(this);
    }
}
