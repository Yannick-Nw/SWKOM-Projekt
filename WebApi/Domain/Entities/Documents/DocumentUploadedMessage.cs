using Domain.Messaging;
using System.Text.Json.Serialization;

namespace Domain.Entities.Documents;

public sealed record DocumentUploadedMessage(DocumentId DocumentId) : IMessage
{
    [JsonIgnore]
    public string Channel { get; } = IMessageQueueService.DOCUMENT_OCR_CHANNEL;
}
