using Infrastructure.Repositories.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Extensions;

public static class AppExtensions
{
    public static IApplicationBuilder UsePaperless(this WebApplication app)
    {
        // Automatically apply migrations and create database on startup
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PaperlessDbContext>();
            dbContext.Database.Migrate(); // Apply migrations automatically
        }

        return app;
    }
}
