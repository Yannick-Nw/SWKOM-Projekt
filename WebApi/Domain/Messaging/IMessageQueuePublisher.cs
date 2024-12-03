using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Messaging;

public interface IMessageQueuePublisher
{
    void Publish<T>(T message) where T : IMessage;
}
