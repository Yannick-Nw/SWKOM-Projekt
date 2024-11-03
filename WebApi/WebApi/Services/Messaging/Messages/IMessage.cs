namespace WebApi.Services.Messaging.Messages;

public interface IMessage
{
    string Channel { get; }
}
