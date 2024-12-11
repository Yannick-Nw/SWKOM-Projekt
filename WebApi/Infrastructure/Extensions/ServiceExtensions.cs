using Amazon.S3;
using Application.Services.Documents;
using Domain.Messaging;
using Domain.Repositories;
using Infrastructure.FileStorage;
using Infrastructure.Messaging;
using Infrastructure.Repositories.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceExtensions
{
    public static IServiceCollection AddRepositoryEfCore(this IServiceCollection services, string connectionString)
    {
        services
            .AddDbContext<PaperlessDbContext>(options => options.UseNpgsql(connectionString))
            .AddScoped<IDocumentRepository, EfCoreDocumentRepository>();

        return services;
    }

    public static IServiceCollection AddFileStorageMinIO(this IServiceCollection services, string host, int port, string accessKey, string secretKey)
    {
        var s3Config = new AmazonS3Config
        {
            ServiceURL = $"http://{host}:{port}",
            ForcePathStyle = true // Required for MinIO
        };

        services
            .AddSingleton(new AmazonS3Client(accessKey, secretKey, s3Config))
            .AddScoped<IDocumentFileStorageService, MinIODocumentFileStorageService>();

        return services;
    }

    public static IServiceCollection AddMessagingRabbitMq(this IServiceCollection services, string host, int port, string username, string password)
    {
        services.AddSingleton<IConnectionFactory>(sp =>
            new ConnectionFactory
            {
                HostName = host,
                Port = port,
                UserName = username,
                Password = password
            }
        );

        services.AddSingleton<IMessageQueueListener, RabbitMqMessageQueueService>();
        services.AddSingleton<IMessageQueuePublisher, RabbitMqMessageQueueService>();

        return services;
    }
}
