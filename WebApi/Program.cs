using WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging();

// CORS konfigurieren, um Anfragen von localhost:80 (WebUI) zuzulassen
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        builder => builder
            .WithOrigins("http://localhost") // URL deiner Web-UI
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ILogger>(prov => prov.GetRequiredService<ILoggerFactory>().CreateLogger("Default"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Verwende die CORS-Policy
app.UseCors("AllowLocalhost");

// Map endpoints
app.MapDocumentEndpoints();

// Aktiviere die Verwendung von statischen Dateien
app.UseStaticFiles();

// Aktiviere den Routing-Support
app.UseRouting();


//app.MapGet("/", () => Results.Redirect("/index.html")); // Weiterleitung auf die Hauptseite

app.Run();
