using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Messaging;

public interface IReceivedMessage<T> where T : IMessage
{
    T Message { get; }

    void Ack();
    void Nack(bool requeue);
}