using System.Text.Json.Serialization;

namespace WebApi.Services.Messaging.Messages;

public interface IMessage
{
    [JsonIgnore]
    string Channel { get; }
}
