using Application.Services.Documents;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services
            .AddScoped<IDocumentService, DocumentService>();

        return services;
    }
}
