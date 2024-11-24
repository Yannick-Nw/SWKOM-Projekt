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
    public void Publish_ShouldPublishMessageToQueue()
    {
        // Arrange
        var mockConnectionFactory = new Mock<IConnectionFactory>();
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IModel>();
        var mockLogger = new Mock<ILogger<RabbitMqMessageQueueService>>();

        mockConnectionFactory
            .Setup(factory => factory.CreateConnection())
            .Returns(mockConnection.Object);

        mockConnection
            .Setup(connection => connection.CreateModel())
            .Returns(mockChannel.Object);

        var service = new RabbitMqMessageQueueService(mockConnectionFactory.Object, mockLogger.Object);

        var testMessage = new TestMessage("Test Content");

        // Act
        service.Publish(testMessage);

        // Assert
        mockChannel.Verify(channel => channel.BasicPublish(
            It.IsAny<string>(),
            It.Is<string>(TestMessage.Channel, StringComparer.Ordinal),
            It.IsAny<bool>(),
            It.IsAny<IBasicProperties>(),
            It.Is<ReadOnlyMemory<byte>>(body => JsonSerializer.Deserialize<TestMessage>(Encoding.UTF8.GetString(body.ToArray()), new JsonSerializerOptions()) == testMessage) // Convert back to object for equality comparison
        ), Times.Once);
    }

    [Fact]
    public void Dispose_ShouldCloseAndDisposeChannelAndConnection()
    {
        // Arrange
        var mockConnectionFactory = new Mock<IConnectionFactory>();
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IModel>();
        var mockLogger = new Mock<ILogger<RabbitMqMessageQueueService>>();

        mockConnectionFactory
            .Setup(factory => factory.CreateConnection())
            .Returns(mockConnection.Object);

        mockConnection
            .Setup(connection => connection.CreateModel())
            .Returns(mockChannel.Object);

        var service = new RabbitMqMessageQueueService(mockConnectionFactory.Object, mockLogger.Object);

        // Act
        service.Dispose();

        // Assert
        mockChannel.Verify(channel => channel.Close(), Times.Once);
        mockChannel.Verify(channel => channel.Dispose(), Times.Once);
        mockConnection.Verify(connection => connection.Close(), Times.Once);
        mockConnection.Verify(connection => connection.Dispose(), Times.Once);
    }

    // Helper class for testing
    private record TestMessage(string Content) : IMessage
    {
        public static string Channel => IMessageQueueService.DOCUMENT_OCR_CHANNEL;
    }
}
