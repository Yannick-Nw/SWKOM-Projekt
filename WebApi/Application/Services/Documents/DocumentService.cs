using Application.Interfaces;
using Application.Interfaces.Files;
using Domain.Entities.Documents;
using Domain.Messaging;
using Domain.Repositories.Documents;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Documents;

public class DocumentService(IDocumentRepository documentRepository, IMessageQueuePublisher messageQueuePublisher, IDocumentFileStorageService fileStorageService, ILogger<DocumentService> logger) : IDocumentService
{
    public async Task CreateAsync(Document document, IFile file, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(document);

        var documentFile = new DocumentFile(document.Id, file);
        try
        {
            // Upload file
            await fileStorageService.UploadAsync(documentFile, ct);

            // Create in database
            await documentRepository.CreateAsync(document, ct);
        } catch (Exception ex)
        {
            // Rollback
            await fileStorageService.DeleteAsync(document.Id, CancellationToken.None);
            await documentRepository.DeleteAsync(document.Id, CancellationToken.None);

            logger.LogError(ex, "Failed to upload document \"{documentId}\" and create in database.", document.Id);
            throw new ApplicationException("Failed upload of document", ex);
        }

        // Publish message to RabbitMQ
        var message = new DocumentUploadedMessage(document.Id);
        await messageQueuePublisher.PublishAsync(message); // Publishes the document info for OCR processing

        logger.LogInformation("Document \"{documentId}\" uploaded and message sent to queue.", document.Id);
    }

    public async Task<bool> DeleteAsync(DocumentId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        return await documentRepository.DeleteAsync(id, ct);
    }

    public async Task<IReadOnlyList<Document>> GetAsync(CancellationToken ct = default)
    {
        return await documentRepository.GetAsync(ct);
    }

    public async Task<Document?> GetAsync(DocumentId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        return await documentRepository.GetAsync(id, ct);
    }
}
