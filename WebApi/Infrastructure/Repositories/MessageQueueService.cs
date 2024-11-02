using RabbitMQ.Client;
using System.Text;

public class MessageQueueService
{
    private readonly IConnectionFactory _connectionFactory;
    private IModel? _channel;
    private IConnection? _connection;

    public MessageQueueService(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
        _connection = _connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: "document_queue", durable: false, exclusive: false, autoDelete: false,
            arguments: null);
    }

    public void PublishMessage(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: string.Empty, routingKey: "document_queue", basicProperties: null, body: body);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}