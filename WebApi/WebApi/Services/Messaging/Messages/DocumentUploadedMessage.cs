using Domain.Entities;
using System.Text.Json.Serialization;

namespace WebApi.Services.Messaging.Messages;

public record DocumentUploadedMessage(DocumentId DocumentId, string Path) : IMessage
{
    [JsonIgnore]
    public string Channel { get; } = "document_ocr_queue";
}
