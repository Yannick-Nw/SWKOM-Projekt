using System.Text.Json.Serialization;

namespace Domain.Messaging;

public interface IMessage
{
    static abstract string Channel { get; }
}
