using WebApi.Services.Messaging.Messages;

namespace WebApi.Services.Messaging;

public interface IMessageQueueService : IDisposable
{
    const string DOCUMENT_OCR_CHANNEL = "document_ocr_queue";

    void Publish<T>(T message) where T : IMessage;
}