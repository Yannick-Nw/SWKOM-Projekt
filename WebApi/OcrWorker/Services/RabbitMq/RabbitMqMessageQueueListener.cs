using Domain.Messaging;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OcrWorker.Services.RabbitMq;

public record ReceivedMessage<T>(T Message, IModel Channel, ulong DeliveryTag) : IReceivedMessage<T> where T : IMessage
{
    public void Ack()
    {
        Channel.BasicAck(DeliveryTag, multiple: false);
    }

    public void Nack(bool requeue)
    {
        Channel.BasicNack(DeliveryTag, multiple: false, requeue);
    }
}

public sealed class RabbitMqMessageQueueListener : IMessageQueueListener
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger _logger;
    public RabbitMqMessageQueueListener(IConnectionFactory connectionFactory, ILogger<RabbitMqMessageQueueListener> logger)
    {
        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
        _logger = logger;
    }

    public async IAsyncEnumerable<IReceivedMessage<T>> ContiniousListenAsync<T>([EnumeratorCancellation] CancellationToken ct = default) where T : IMessage
    {
        _channel.QueueDeclare(queue: T.Channel, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var messageChannel = Channel.CreateUnbounded<IReceivedMessage<T>>();

        var consumer = new EventingBasicConsumer(_channel);

        // Start consuming messages
        var consumerTag = _channel.BasicConsume(
            queue: T.Channel,
            autoAck: false,
            consumer: consumer);

        consumer.Received += (model, ea) =>
        {
            try
            {
                // Deserialize the message
                var body = ea.Body.ToArray();
                var messageString = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<T>(messageString) ?? throw new Exception("Failed to deserialize message");

                // Write the message to the channel
                var receivedMessage = new ReceivedMessage<T>(message, _channel, ea.DeliveryTag);
                messageChannel.Writer.TryWrite(receivedMessage);
            } catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "Error processing message");

                // Nack the message
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        // Complete the channel when the consumer is canceled
        consumer.ConsumerCancelled += (sender, args) =>
        {
            _channel.BasicCancel(consumerTag);
            messageChannel.Writer.TryComplete();
        };

        // Register cancellation to stop consuming messages
        ct.Register(() =>
        {
            _channel.BasicCancel(consumerTag);
            messageChannel.Writer.TryComplete();
        });

        try
        {
            await foreach (var message in messageChannel.Reader.ReadAllAsync(ct))
            {
                yield return message;
            }
        } finally
        {
            // Ensure the consumer is canceled and the channel is completed
            _channel.BasicCancel(consumerTag);
            messageChannel.Writer.TryComplete();
        }
    }
    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();

        GC.SuppressFinalize(this);
    }
}
