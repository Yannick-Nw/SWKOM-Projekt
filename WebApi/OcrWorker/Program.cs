
// Retrieve environemnt variables to connect to RabbitMQ queue
using Application.Services.Documents;
using Domain.Entities.Documents;
using Domain.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OcrWorker.Extensions;
using OcrWorker.Services.Ocr;
using RabbitMQ.Client;

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

// Listen to queue
logger.LogInformation("Listening to queue");
await foreach (var receivedMessage in queueListener.ListenAsync<DocumentUploadedMessage>())
{
    var documentId = receivedMessage.Message.DocumentId;

    // Retrieve document file
    var documentFile = await documentFileStorageService.GetAsync(documentId);
    if (documentFile is null)
    {
        logger.LogError("Document file not found");
        await receivedMessage.NackAsync(requeue: false);
        continue;
    }

    // Perform OCR
    var result = await ocrService.ProcessAsync(documentFile.File);

    // Log result and later SAVE
    logger.LogInformation("OCR result: {result}", result);

    await receivedMessage.AckAsync();
}