namespace Domain.Messaging;

public interface IMessageQueueService : IDisposable
{
    const string DOCUMENT_OCR_CHANNEL = "document_ocr_queue";

    Task PublishAsync<T>(T message) where T : IMessage;
}