using System.Text.Json.Serialization;

namespace Domain.Messaging;

public interface IMessage
{
    [JsonIgnore]
    string Channel { get; }
}
