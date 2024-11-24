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

public class RabbitMqMessageQueueService : IMessageQueueService
{
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly ILogger _logger;

    public RabbitMqMessageQueueService(IConnectionFactory connectionFactory, ILogger<RabbitMqMessageQueueService> logger)
    {
        _connection = connectionFactory.CreateConnection();
        _logger = logger;

        _channel = _connection.CreateModel();

        // Create queue
        _channel.QueueDeclare(queue: IMessageQueueService.DOCUMENT_OCR_CHANNEL, durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public void Publish<T>(T message) where T : IMessage
    {
        // Convert message to json byte array
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        _channel.BasicPublish(exchange: string.Empty, routingKey: message.Channel, basicProperties: null, body: body);
        _logger.LogInformation("Message published: {message}", message);
    }

    public void Dispose()
    {
        _channel.Close();
        _connection.Close();

        _channel.Dispose();
        _connection.Dispose();
    }
}
