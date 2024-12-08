// Retrieve environemnt variables to connect to RabbitMQ queue

using Application.Services.Documents;
using Domain.Entities.Documents;
using Domain.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OcrWorker.Extensions;
using OcrWorker.Services;
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
    .AddSingleton<ElasticSearchClient>() // Add ElasticSearch client
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

    // Perform OCR on the file
    var ocrResult = await ocrService.ProcessAsync(documentFile.File);

    // Log the OCR result
    logger.LogInformation("OCR result: {ocrResult}", ocrResult);

    // Index OCR result into ElasticSearch
    try
    {
        var elasticSearchClient = serviceProvider.GetRequiredService<ElasticSearchClient>();
        await elasticSearchClient.IndexDocumentAsync("documents",
            new
            {
                Id = documentId,
                FileName = "programdotcs_filename_change_me",
                ContentType = documentFile.File.ContentType,
                Text = ocrResult,
                ProcessedAt = DateTime.UtcNow
            });

        logger.LogInformation("Document indexed in ElasticSearch.");
    } catch (Exception ex)
    {
        logger.LogError("Failed to index document in ElasticSearch: {ex}", ex.Message);
    }

    // Acknowledge the message
    await receivedMessage.AckAsync();
}