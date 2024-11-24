using Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using OcrWorker.Services.Ocr;
using OcrWorker.Services.RabbitMq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcrWorker.Extensions;
public static class ServiceExtensions
{
    public static IServiceCollection AddOcrServices(this IServiceCollection services)
    {
        var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
        var rabbitMqPort = Environment.GetEnvironmentVariable("RABBITMQ_PORT");
        var rabbitMqUser = Environment.GetEnvironmentVariable("RABBITMQ_USER");
        var rabbitMqPass = Environment.GetEnvironmentVariable("RABBITMQ_PASS");

        var minIoHost = Environment.GetEnvironmentVariable("MINIO_HOST");
        var minIoPort = Environment.GetEnvironmentVariable("MINIO_PORT");
        var minIoAccessKey = Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY");
        var minIoSecretKey = Environment.GetEnvironmentVariable("MINIO_SECRET_KEY");

        // Validate are all set
        if (string.IsNullOrEmpty(rabbitMqHost) || string.IsNullOrEmpty(rabbitMqPort) || string.IsNullOrEmpty(rabbitMqUser) || string.IsNullOrEmpty(rabbitMqPass))
            throw new InvalidOperationException("RabbitMQ connection details are not set");

        if (string.IsNullOrEmpty(minIoHost) || string.IsNullOrEmpty(minIoPort) || string.IsNullOrEmpty(minIoAccessKey) || string.IsNullOrEmpty(minIoSecretKey))
            throw new InvalidOperationException("MinIO connection details are not set");

        services
            .AddLogging()
            .AddMessagingRabbitMq(rabbitMqHost, int.Parse(rabbitMqPort), rabbitMqUser, rabbitMqPass)
            .AddFileStorageMinIO(minIoHost, int.Parse(minIoPort), minIoAccessKey, minIoSecretKey)
            .AddScoped<IOcrProcessorService, TesseractOcrProcessorService>()
            .AddScoped<IMessageQueueListener, RabbitMqMessageQueueListener>();

        return services;
    }
}
