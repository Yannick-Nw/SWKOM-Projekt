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
using Amazon.S3;
using Amazon;

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

    public static IServiceCollection AddFileStorageMinIO(this IServiceCollection services)
    {
        services
            .AddScoped<IDocumentFileStorageService, MinIOFileStorageService>();

        return services;
    }

    public static IServiceCollection AddMessagingRabbitMq(this IServiceCollection services, string host, int port,
        string username, string password)
    {
        services.AddSingleton<IConnectionFactory>(sp =>
            new ConnectionFactory { HostName = host, Port = port, UserName = username, Password = password }
        );

        services.AddSingleton<IMessageQueueService, RabbitMqMessageQueueService>();

        return services;
    }

    public static IServiceCollection AddMinIO(this IServiceCollection services, IConfiguration configuration)
    {
        var minIOConfig = configuration.GetSection("MinIOSettings").Get<MinIOConfiguration>();

        var s3Config = new AmazonS3Config
        {
            ServiceURL = minIOConfig.ServiceUrl, ForcePathStyle = true // Required for MinIO
        };

        var s3Client = new AmazonS3Client(minIOConfig.AccessKey, minIOConfig.SecretKey, s3Config);

        services.AddSingleton(s3Client);
        services.AddSingleton(minIOConfig);

        return services;
    }
}