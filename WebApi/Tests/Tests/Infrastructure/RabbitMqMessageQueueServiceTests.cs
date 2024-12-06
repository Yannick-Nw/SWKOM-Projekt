using Domain.Messaging;
using Infrastructure.Messaging;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace Tests.Tests.Infrastructure;


public class RabbitMqMessageQueueServiceTests
{
    [Fact]
    public async Task Publish_ShouldPublishMessageToQueue()
    {
        // Arrange
        var mockConnectionFactory = new Mock<IConnectionFactory>();
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IChannel>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();

        mockConnectionFactory
            .Setup(factory => factory.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockConnection.Object);

        mockConnection
            .Setup(connection => connection.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockChannel.Object);

        mockLoggerFactory
            .Setup(factory => factory.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);

        var service = new RabbitMqMessageQueueService(mockConnectionFactory.Object, mockLoggerFactory.Object);

        var testMessage = new TestMessage("Test Content");

        // Act
        await service.PublishAsync(testMessage);

        // Assert
        mockChannel.Verify(channel => channel.BasicPublishAsync<BasicProperties>(
            It.IsAny<string>(),
            It.Is<string>(TestMessage.Channel, StringComparer.Ordinal),
            It.IsAny<bool>(),
            It.IsAny<BasicProperties>(),
            It.Is<ReadOnlyMemory<byte>>(body => JsonSerializer.Deserialize<TestMessage>(Encoding.UTF8.GetString(body.ToArray()), new JsonSerializerOptions()) == testMessage), // Convert back to object for equality comparison
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public void Dispose_ShouldCloseAndDisposeChannelAndConnection()
    {
        // Arrange
        var mockConnectionFactory = new Mock<IConnectionFactory>();
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IChannel>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();

        mockConnectionFactory
            .Setup(factory => factory.CreateConnectionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockConnection.Object);

        mockConnection
            .Setup(connection => connection.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockChannel.Object);

        mockLoggerFactory
            .Setup(factory => factory.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);

        var service = new RabbitMqMessageQueueService(mockConnectionFactory.Object, mockLoggerFactory.Object);

        // Act
        service.Dispose();

        // Assert
        mockChannel.Verify(channel => channel.Dispose(), Times.Once);
        mockConnection.Verify(connection => connection.Dispose(), Times.Once);
    }

    // Helper class for testing
    private record TestMessage(string Content) : IMessage
    {
        public static string Channel => IMessageQueueService.DOCUMENT_OCR_CHANNEL;
    }
}
