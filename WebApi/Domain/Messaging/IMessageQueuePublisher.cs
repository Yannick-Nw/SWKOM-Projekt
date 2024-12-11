using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Messaging;

public interface IMessageQueuePublisher
{
    Task PublishAsync<T>(T message) where T : IMessage;
}
