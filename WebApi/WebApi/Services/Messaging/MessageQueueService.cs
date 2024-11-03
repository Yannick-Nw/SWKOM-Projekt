using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using WebApi.Services.Messaging.Messages;

namespace WebApi.Services.Messaging;

public class MessageQueueService : IMessageQueueService
{
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly ILogger _logger;

    public MessageQueueService(IConnectionFactory connectionFactory, ILogger<MessageQueueService> logger)
    {
        _connection = connectionFactory.CreateConnection();
        _logger = logger;

        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: "document_ocr_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
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