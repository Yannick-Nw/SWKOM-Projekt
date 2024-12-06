using Domain.Messaging;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Infrastructure.Messaging;


public record ReceivedMessage<T>(T Message, IChannel Channel, ulong DeliveryTag) : IReceivedMessage<T> where T : IMessage
{
    public async Task AckAsync()
    {
        await Channel.BasicAckAsync(DeliveryTag, multiple: false);
    }

    public async Task NackAsync(bool requeue)
    {
        await Channel.BasicNackAsync(DeliveryTag, multiple: false, requeue);
    }
}

public class RabbitMqAsyncConsumer<T>(IChannel channel, Channel<IReceivedMessage<T>> messageChannel, ILogger<RabbitMqAsyncConsumer<T>> logger) : AsyncDefaultBasicConsumer(channel) where T : IMessage
{
    public override async Task HandleBasicDeliverAsync(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IReadOnlyBasicProperties properties, ReadOnlyMemory<byte> body, CancellationToken cancellationToken = default)
    {
        await base.HandleBasicDeliverAsync(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body, cancellationToken);

        try
        {
            // Deserialize the message
            var messageString = Encoding.UTF8.GetString(body.ToArray());
            var message = JsonSerializer.Deserialize<T>(messageString) ?? throw new Exception("Failed to deserialize message");

            // Write the message to the channel
            var receivedMessage = new ReceivedMessage<T>(message, Channel, deliveryTag);
            messageChannel.Writer.TryWrite(receivedMessage);
        } catch (Exception ex)
        {
            // Log the exception
            logger.LogError(ex, "Error processing message");

            // Nack the message
            await Channel.BasicNackAsync(deliveryTag, multiple: false, requeue: true);
        }
    }

    public override async Task HandleBasicCancelAsync(string consumerTag, CancellationToken cancellationToken = default)
    {
        await base.HandleBasicCancelAsync(consumerTag, cancellationToken);

        messageChannel.Writer.TryComplete();
    }
}

public sealed class RabbitMqMessageQueueService : IMessageQueuePublisher, IMessageQueueListener
{
    private readonly IChannel _publishChannel;
    private readonly IConnection _connection;
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;

    public RabbitMqMessageQueueService(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
    {
        _connection = connectionFactory.CreateConnectionAsync().Result;
        _publishChannel = _connection.CreateChannelAsync().Result;
        _logger = loggerFactory.CreateLogger<RabbitMqMessageQueueService>();
        _loggerFactory = loggerFactory;
    }

    public async Task PublishAsync<T>(T message) where T : IMessage
    {
        await _publishChannel.QueueDeclareAsync(queue: T.Channel, durable: true, exclusive: false, autoDelete: false, arguments: null);

        // Convert message to json byte array
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        await _publishChannel.BasicPublishAsync(exchange: string.Empty, routingKey: T.Channel, mandatory: false, basicProperties: new BasicProperties(), body: body);
        _logger.LogInformation("Message published: {message}", message);
    }

    public async IAsyncEnumerable<IReceivedMessage<T>> ListenAsync<T>([EnumeratorCancellation] CancellationToken ct = default) where T : IMessage
    {
        using var channel = await _connection.CreateChannelAsync(cancellationToken: ct);

        await channel.QueueDeclareAsync(queue: T.Channel, durable: true, exclusive: false, autoDelete: false, arguments: null, noWait: false, ct);

        var messageChannel = Channel.CreateUnbounded<IReceivedMessage<T>>();

        var consumer = new RabbitMqAsyncConsumer<T>(channel, messageChannel, _loggerFactory.CreateLogger<RabbitMqAsyncConsumer<T>>());

        // Start consuming messages
        var consumerTag = await channel.BasicConsumeAsync(
            queue: T.Channel,
            autoAck: false,
            consumer: consumer);

        // Register cancellation to stop consuming messages
        ct.Register(async () =>
        {
            await channel.BasicCancelAsync(consumerTag, cancellationToken: CancellationToken.None);
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
            await channel.BasicCancelAsync(consumerTag, cancellationToken: CancellationToken.None);
            messageChannel.Writer.TryComplete();
        }
    }



    public void Dispose()
    {
        _publishChannel.Dispose();
        _connection.Dispose();

        GC.SuppressFinalize(this);
    }
}
