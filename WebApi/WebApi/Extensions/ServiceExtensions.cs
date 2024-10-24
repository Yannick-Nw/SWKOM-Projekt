﻿using Domain.Repositories;
using Infrastructure;
using Infrastructure.Repositories.EntityFrameworkCore;
using Infrastructure.Repositories.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;
using WebApi.Mappings;

namespace WebApi.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection RegisterPaperless(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .RegisterMappings()
            .RegisterLogging()
            .RegisterDatabase(configuration.GetConnectionString("DefaultConnection")!)
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
}
