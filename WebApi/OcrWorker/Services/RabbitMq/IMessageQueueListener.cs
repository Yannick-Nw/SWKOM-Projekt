using Domain.Messaging;
using System.Runtime.CompilerServices;

namespace OcrWorker.Services.RabbitMq;
public interface IMessageQueueListener : IDisposable
{
    IAsyncEnumerable<IReceivedMessage<T>> ContiniousListenAsync<T>(CancellationToken ct = default) where T : IMessage;
}