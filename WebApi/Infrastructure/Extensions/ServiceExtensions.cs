using Amazon.S3;
using Application.Services.Documents;
using Domain.Messaging;
using Domain.Repositories.Documents;
using Domain.Services.Documents;
using Elastic.Clients.Elasticsearch;
using Infrastructure.FileStorage.MinIO;
using Infrastructure.Messaging.RabbitMq;
using Infrastructure.Repositories.EfCore;
using Infrastructure.Services.ElasticSearch;
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
    public static IServiceCollection AddEfCoreRepository(this IServiceCollection services, string connectionString)
    {
        services
            .AddDbContext<PaperlessDbContext>(options => options.UseNpgsql(connectionString))
            .AddScoped<IDocumentRepository, EfCoreDocumentRepository>();

        return services;
    }

    public static IServiceCollection AddMinIOFileStorage(this IServiceCollection services, string host, int port, string accessKey, string secretKey)
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

    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, string host, int port, string username, string password)
    {
        var connectionFactory = new ConnectionFactory
            {
                HostName = host,
                Port = port,
                UserName = username,
                Password = password
            };
        services
            .AddSingleton<IConnectionFactory>(connectionFactory)
            .AddSingleton<IMessageQueueListener, RabbitMqMessageQueueService>()
            .AddSingleton<IMessageQueuePublisher, RabbitMqMessageQueueService>();

        return services;
    }

    public static IServiceCollection AddServiceElasticSearch(this IServiceCollection services, string host, int port)
    {
        var settings = new ElasticsearchClientSettings(new Uri($"http://{host}:{port}"));

        services
            .AddSingleton<IElasticsearchClientSettings>(settings)
            .AddScoped<IDocumentIndexService, ElasticSearchDocumentIndexedService>();

        return services;
    }
}
