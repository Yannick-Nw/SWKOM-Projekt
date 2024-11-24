using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace OcrWorker;

public class OcrWorker
{
    private readonly IModel _channel;
    private readonly OcrService _ocrService;
    private readonly AmazonS3Client _s3Client;

    public OcrWorker(OcrService ocrService)
    {
        _ocrService = ocrService;

        // RabbitMQ connection
        var factory = new ConnectionFactory { HostName = "rabbitmq" };
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();
        _channel.QueueDeclare("OCR_QUEUE", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueDeclare("RESULT_QUEUE", durable: true, exclusive: false, autoDelete: false);

        // S3 setup
        var s3Config = new AmazonS3Config { ServiceURL = "http://minio:9000" };
        _s3Client = new AmazonS3Client("minio", "minio123", s3Config);
    }

    public void StartListening()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var document = JsonSerializer.Deserialize<DocumentMessage>(message);

            // Process OCR
            var filePath = await DownloadFileFromS3(document.ObjectKey);
            var ocrResult = _ocrService.PerformOcr(filePath);

            // Publish OCR Result
            var resultMessage = JsonSerializer.Serialize(new
            {
                DocumentId = document.DocumentId,
                TextContent = ocrResult
            });
            _channel.BasicPublish("", "RESULT_QUEUE", null, Encoding.UTF8.GetBytes(resultMessage));
        };

        _channel.BasicConsume("OCR_QUEUE", true, consumer);
    }

    private async Task<string> DownloadFileFromS3(string objectKey)
    {
        var request = new GetObjectRequest { BucketName = "documents", Key = objectKey };
        var response = await _s3Client.GetObjectAsync(request);
        var filePath = $"/tmp/{objectKey}";
        await response.WriteResponseStreamToFileAsync(filePath, false);
        return filePath;
    }
}

public class DocumentMessage
{
    public string DocumentId { get; set; }
    public string ObjectKey { get; set; }
}

/*
private readonly ILogger<OcrWorker> _logger;

public OcrWorker(ILogger<OcrWorker> logger)
{
    _logger = logger;
}

protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("OcrWorker running at: {time}", DateTimeOffset.Now);
        }

        await Task.Delay(1000, stoppingToken);
    }
}*/