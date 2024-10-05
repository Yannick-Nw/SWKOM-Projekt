using WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging();

// CORS konfigurieren, um Anfragen von localhost:80 (WebUI) zuzulassen
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebUI",
        policy =>
        {
            policy.WithOrigins("http://localhost") // Die URL deiner Web-UI
                .AllowAnyHeader()
                .AllowAnyOrigin()
                .AllowAnyMethod();
        });
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
app.UseCors("AllowWebUI");

// Map endpoints
app.MapDocumentEndpoints();

//app.MapGet("/", () => Results.Redirect("/index.html")); // Weiterleitung auf die Hauptseite

app.Run();
