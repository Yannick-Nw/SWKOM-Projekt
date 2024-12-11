using Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OcrWorker.Services.Ocr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcrWorker.Extensions;
public static class ServiceExtensions
{
    public static IServiceCollection AddOcrServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure RabbitMQ connection settings
        var rabbitMqHost = configuration.GetValue<string>("RABBITMQ:HOST", "localhost")!;
        int rabbitMqPort = configuration.GetValue<int>("RABBITMQ:PORT", 5672);
        var rabbitMqUsername = configuration.GetValue<string>("RABBITMQ:USER", string.Empty)!;
        var rabbitMqPassword = configuration.GetValue<string>("RABBITMQ:PASS", string.Empty)!;

        // MinIO settings
        var minIoHost = configuration.GetValue<string>("MINIO:HOST", "localhost")!;
        int minIoPort = configuration.GetValue<int>("MINIO:PORT", 9000);
        var minIoAccessKey = configuration.GetValue<string>("MINIO:ACCESS_KEY", string.Empty)!;
        var minIoSecretKey = configuration.GetValue<string>("MINIO:SECRET_KEY", string.Empty)!;

        // Elasticsearch settings
        var elasticSearchHost = configuration.GetValue<string>("ELASTICSEARCH:HOST", "localhost")!;
        int elasticSearchPort = configuration.GetValue<int>("ELASTICSEARCH:PORT", 9200);

        // Validate are all set
        if (string.IsNullOrEmpty(rabbitMqHost) || string.IsNullOrEmpty(rabbitMqUsername) || string.IsNullOrEmpty(rabbitMqPassword))
            throw new InvalidOperationException("RabbitMQ connection details are not set");

        if (string.IsNullOrEmpty(minIoHost) || string.IsNullOrEmpty(minIoAccessKey) || string.IsNullOrEmpty(minIoSecretKey))
            throw new InvalidOperationException("MinIO connection details are not set");

        if (string.IsNullOrEmpty(elasticSearchHost))
            throw new InvalidOperationException("ElasticSearch connection details are not set");

        services
            .AddLogging()
            .AddRabbitMqMessaging(rabbitMqHost, rabbitMqPort, rabbitMqUsername, rabbitMqPassword)
            .AddMinIOFileStorage(minIoHost, minIoPort, minIoAccessKey, minIoSecretKey)
            .AddServiceElasticSearch(elasticSearchHost, elasticSearchPort)
            //.AddScoped<IOcrProcessorService, TesseractOcrProcessorService>();
            .AddScoped<IOcrProcessorService, FakeOcrProcessorService>();

        return services;
    }
}
