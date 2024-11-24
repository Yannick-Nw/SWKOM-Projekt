using Domain.Repositories;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using WebApi.Mappings;
using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;
using Infrastructure.Extensions;
using Infrastructure.Repositories.EfCore;

namespace WebApi.Extensions;


[ExcludeFromCodeCoverage]
public static class ServiceExtensions
{
    public static IServiceCollection RegisterPaperless(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddMappings()
            .AddLogging()
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
        var rabbitMqHost = configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
        int rabbitMqPort = configuration.GetValue<int>("RabbitMQ:Port", 5672); // Set default 5672 here
        var rabbitMqUsername = configuration.GetValue<string>("RabbitMQ:Username") ?? "guest";
        var rabbitMqPassword = configuration.GetValue<string>("RabbitMQ:Password") ?? "guest";
        // MinIO settings


        services
            .AddRepositoryEfCore(connectionString)
            .AddMessagingRabbitMq(rabbitMqHost, rabbitMqPort, rabbitMqUsername, rabbitMqPassword)
            .AddFileStorageMinIO();

        return services;
    }
}