using Microsoft.EntityFrameworkCore;
using WebApi.Endpoints;
using WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Allow requests from WebApp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder => builder
            .WithOrigins("http://localhost") // URL of WebApp
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.RegisterPaperless(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapDocumentEndpoints();
app.UseRouting();

app.UsePaperless();

app.Run();
