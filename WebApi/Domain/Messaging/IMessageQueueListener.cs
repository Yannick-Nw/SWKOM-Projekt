using System.Runtime.CompilerServices;

namespace Domain.Messaging;

public interface IMessageQueueListener
{
    IAsyncEnumerable<IReceivedMessage<T>> ListenAsync<T>(CancellationToken ct = default) where T : IMessage;
}