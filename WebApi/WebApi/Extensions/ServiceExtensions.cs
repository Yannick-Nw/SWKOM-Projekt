using Domain.Repositories;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using WebApi.Mappings;
using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;
using Infrastructure.Extensions;
using Infrastructure.Repositories.EfCore;
using Application.Services.Documents;
using System.Net.Sockets;

namespace WebApi.Extensions;


[ExcludeFromCodeCoverage]
public static class ServiceExtensions
{
    public static IServiceCollection RegisterPaperless(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddMappings()
            .AddLogging()
            .AddScoped<IDocumentService, DocumentService>()
            .AddInfrastructure(configuration);

        return services;
    }

    private static IServiceCollection AddMappings(this IServiceCollection services)
    {
        services.AddAutoMapper(c =>
        {
            c.AddProfile<PaperlessProfile>();
            c.AddProfile<InfrastructureProfile>();
        });

        return services;
    }

    private static IServiceCollection AddLogging(this IServiceCollection services)
    {
        LoggingServiceCollectionExtensions.AddLogging(services);
        services.AddScoped<ILogger>(prov => prov.GetRequiredService<ILoggerFactory>().CreateLogger("Default"));

        return services;
    }

    private static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
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

        services
            .AddRepositoryEfCore(connectionString)
            .AddMessagingRabbitMq(rabbitMqHost, rabbitMqPort, rabbitMqUsername, rabbitMqPassword)
            .AddFileStorageMinIO(minIoHost, minIoPort, minIoAccessKey, minIoSecretKey);

        return services;
    }
}