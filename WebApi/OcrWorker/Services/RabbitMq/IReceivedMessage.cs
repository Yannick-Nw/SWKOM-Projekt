
using Domain.Messaging;

namespace OcrWorker.Services.RabbitMq;

public interface IReceivedMessage<T> where T : IMessage
{
    T Message { get; }

    void Ack();
    void Nack(bool requeue);
}