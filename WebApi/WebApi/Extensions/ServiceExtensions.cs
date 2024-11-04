using Domain.Repositories;
using Infrastructure;
using Infrastructure.Repositories;
using Infrastructure.Repositories.EntityFrameworkCore;
using Infrastructure.Repositories.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using WebApi.Mappings;
using RabbitMQ.Client;
using WebApi.Services.Messaging;
using System.Diagnostics.CodeAnalysis;

namespace WebApi.Extensions;


[ExcludeFromCodeCoverage]
public static class ServiceExtensions
{
    public static IServiceCollection RegisterPaperless(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .RegisterMappings()
            .RegisterLogging()
            .RegisterDatabase(configuration.GetConnectionString("DefaultConnection")!)
            .RegisterMessageQueue(configuration) // Registers RabbitMQ
            .AddScoped<IDocumentRepository, DocumentRepository>();

        return services;
    }

    private static IServiceCollection RegisterMappings(this IServiceCollection services)
    {
        services.AddAutoMapper(c =>
        {
            c.AddProfile<PaperlessProfile>();
            c.AddProfile<InfrastructureProfile>();
        });

        return services;
    }

    private static IServiceCollection RegisterLogging(this IServiceCollection services)
    {
        services.AddLogging();
        services.AddScoped<ILogger>(prov => prov.GetRequiredService<ILoggerFactory>().CreateLogger("Default"));

        return services;
    }

    private static IServiceCollection RegisterDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PaperlessDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }

    private static IServiceCollection RegisterMessageQueue(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure RabbitMQ connection settings
        var rabbitMqHost = configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
        int rabbitMqPort = configuration.GetValue<int>("RabbitMQ:Port", 5672); // Set default 5672 here

        services.AddSingleton<IConnectionFactory>(sp =>
            new ConnectionFactory
            {
                HostName = rabbitMqHost,
                Port = rabbitMqPort,
                UserName = configuration.GetValue<string>("RabbitMQ:Username") ?? "guest",
                Password = configuration.GetValue<string>("RabbitMQ:Password") ?? "guest"
            }
        );

        // Register MessageQueueService as a singleton
        services.AddSingleton<IMessageQueueService, MessageQueueService>();

        return services;
    }
}