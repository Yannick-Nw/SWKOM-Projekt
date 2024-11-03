using Infrastructure.Repositories.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Extensions;

public static class AppExtensions
{
    public static IApplicationBuilder UsePaperless(this WebApplication app)
    {
        app.PerformAutomaticMigration();

        app.AddExceptionHandlingMiddleware();

        return app;
    }

    private static IApplicationBuilder PerformAutomaticMigration(this WebApplication app)
    {
        // Automatically apply migrations and create database on startup
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PaperlessDbContext>();
            dbContext.Database.Migrate(); // Apply migrations automatically
        }

        return app;
    }

    private static IApplicationBuilder AddExceptionHandlingMiddleware(this WebApplication app)
    {
        // Log exceptions with ILogger
        app.UseExceptionHandler(o =>
        {
            o.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.GetRequiredFeature<IExceptionHandlerPathFeature>();
                var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("ExceptionHandler");

                // Log error
                logger.LogError(exceptionHandlerPathFeature.Error, "An unhandled exception has occurred.");

                await context.Response.WriteAsJsonAsync(new { error = "An error occurred." });
            });
        });

        return app;
    }
}
