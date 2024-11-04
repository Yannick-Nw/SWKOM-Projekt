using Domain.Entities;
using System.Text.Json.Serialization;

namespace WebApi.Services.Messaging.Messages;

public sealed record DocumentUploadedMessage(DocumentId DocumentId, string Path) : IMessage
{
    [JsonIgnore]
    public string Channel { get; } = IMessageQueueService.DOCUMENT_OCR_CHANNEL;
}
