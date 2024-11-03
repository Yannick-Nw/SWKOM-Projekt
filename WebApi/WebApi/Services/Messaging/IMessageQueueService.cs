using WebApi.Services.Messaging.Messages;

namespace WebApi.Services.Messaging;

public interface IMessageQueueService : IDisposable
{
    void Publish<T>(T message) where T : IMessage;
}