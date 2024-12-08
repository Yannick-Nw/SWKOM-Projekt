using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using OcrWorker.Services;
using System.Diagnostics.CodeAnalysis;
using WebApi.Endpoints;
using WebApi.Extensions;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

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

// Register ElasticSearchClient
builder.Services.AddSingleton<ElasticSearchClient>();

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

/// <summary>
///  Exclude this file from code coverage
/// </summary>
[ExcludeFromCodeCoverage]
public partial class Program
{
}