// Retrieve environemnt variables to connect to RabbitMQ queue

using Application.Services.Documents;
using Domain.Entities.Documents;
using Domain.Messaging;
using Domain.Services.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OcrWorker.Extensions;
using OcrWorker.Services.Ocr;
using System.Diagnostics;

// Basic console logger
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

var serviceProvider = new ServiceCollection()
    .AddOcrServices(configuration)
    .BuildServiceProvider();

// Create connection factory
using var queueListener = serviceProvider.GetRequiredService<IMessageQueueListener>();
using var documentFileStorageService = serviceProvider.GetRequiredService<IDocumentFileStorageService>();
using var ocrService = serviceProvider.GetRequiredService<IOcrProcessorService>();
using var documentIndexService = serviceProvider.GetRequiredService<IDocumentIndexService>();

// Listen to queue
logger.LogInformation("Listening to queue");
await foreach (var receivedMessage in queueListener.ListenAsync<DocumentUploadedMessage>())
{
    var documentId = receivedMessage.Message.DocumentId;

    // Retrieve document file
    DocumentFile? documentFile;
    try
    {
        documentFile = await documentFileStorageService.GetAsync(documentId);
        ArgumentNullException.ThrowIfNull(documentFile);
    } catch (Exception ex) {
        logger.LogError(ex, "Retrieve document file failed");
        await receivedMessage.NackAsync(requeue: false);
        continue;
    }

    // OCR file
    string? ocrResult;
    try
    {
        ocrResult = await ocrService.ProcessAsync(documentFile.File);
    } catch (Exception ex)
    {
        logger.LogError(ex, "OCR failed");
        await receivedMessage.NackAsync(requeue: false);
        continue;
    }

    // Log the OCR result
    logger.LogInformation("OCR result: \"{ocrResult}\"", ocrResult);

    // Index OCR result into ElasticSearch
    try
    {
        var success = await documentIndexService.StoreAsync(documentId, ocrResult);
        if (!success) throw new ApplicationException("Unsuccessful index storage");
    } catch (Exception ex)
    {
        logger.LogError(ex, "Document indexing failed");
        await receivedMessage.NackAsync(requeue: false);
        continue;
    }

    logger.LogInformation("Finished processing \"{documentId}\"!", documentId);

    // Acknowledge the message
    await receivedMessage.AckAsync();
}