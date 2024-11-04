using Domain.Entities;
using System.Linq;
using Domain.Repositories;
using Infrastructure.Repositories;
using Infrastructure.Repositories.EntityFrameworkCore;
using Infrastructure.Repositories.EntityFrameworkCore.Dbos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApi.Models;
using System.Globalization;
using Microsoft.OpenApi.Models;
using Domain.Validation;
using WebApi.Services.Messaging;
using WebApi.Services.Messaging.Messages;
using Microsoft.AspNetCore.Http;
using FluentValidation;
using System.Diagnostics.CodeAnalysis;

namespace WebApi.Endpoints;

public static class DocumentEndpoints
{
    /// <summary>
    /// Maps the document endpoints to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointRouteBuilder"/> to map the endpoints to.</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> with the mapped endpoints.</returns>
    [ExcludeFromCodeCoverage]
    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/document");

        group.MapGet("", GetDocumentsAsync)
            .WithOpenApi(c => new(c)
            {
                Summary = "Retrieves all documents.",
                Description = "Retrieves all documents from the database."
            });

        group
            .MapGet("{id:guid}", GetDocumentAsync)
            .WithName("GetDocumentById")
            .WithOpenApi(c => new(c)
            {
                Summary = "Retrieves a document by its ID.",
                Description = "Retrieves a document from the database by its ID.",
                Parameters = new List<OpenApiParameter>
                {
                    new()
                    {
                        Name = "id",
                        In = ParameterLocation.Path,
                        Required = true,
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "uuid"
                        }
                    }
                }
            });

        group
            .MapPost("", UploadDocumentAsync)
            .DisableAntiforgery()
            .WithOpenApi(c => new(c)
            {
                Summary = "Uploads a document.",
                Description = "Uploads a document to the database.",
                RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["file"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary"
                                    },
                                    ["fileName"] = new OpenApiSchema
                                    {
                                        Type = "string"
                                    },
                                    ["title"] = new OpenApiSchema
                                    {
                                        Type = "string"
                                    },
                                    ["author"] = new OpenApiSchema
                                    {
                                        Type = "string"
                                    }
                                }
                            }
                        }
                    }
                }
            });

        group.MapDelete("{id:guid}", DeleteDocumentAsync)
            .WithOpenApi(c => new(c)
            {
                Summary = "Deletes a document by its ID.",
                Description = "Deletes a document from the database by its ID.",
                Parameters = new List<OpenApiParameter>
                {
                    new()
                    {
                        Name = "id",
                        In = ParameterLocation.Path,
                        Required = true,
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "uuid"
                        }
                    }
                }
            });

        return builder;
    }

    public static async Task<Ok<IReadOnlyList<PaperlessDocument>>> GetDocumentsAsync(
        IDocumentRepository documentRepository, ILoggerFactory loggerFactory, CancellationToken ct = default)
    {
        var logger = loggerFactory.CreateLogger(nameof(DocumentEndpoints));

        logger.LogInformation("Fetching all documents");

        var documents = await documentRepository.GetAsync(ct);

        return TypedResults.Ok(documents);
    }

    public static async Task<Results<Ok<PaperlessDocument>, NotFound<Guid>>> GetDocumentAsync([FromRoute] Guid id,
        IDocumentRepository documentRepository, ILoggerFactory loggerFactory, CancellationToken ct = default)
    {
        var logger = loggerFactory.CreateLogger(nameof(DocumentEndpoints));

        logger.LogInformation("Fetching document: {documentId}", id);

        var document = await documentRepository.GetAsync(new DocumentId(id), ct);
        if (document is null)
            return TypedResults.NotFound<Guid>(id);

        return TypedResults.Ok(document);
    }

    public static async Task<Results<CreatedAtRoute, UnprocessableEntity>> UploadDocumentAsync([FromForm] UploadDocumentModel model,
        IDocumentRepository documentRepository, IMessageQueueService messageQueueService, ILoggerFactory loggerFactory,
        CancellationToken ct = default)
    {
        var logger = loggerFactory.CreateLogger(nameof(DocumentEndpoints));

        logger.LogInformation("Uploading document: {model}", model);

        // Upload to S3
        var s3Path = Guid.NewGuid().ToString(); // Mocked

        var fileName = model.FileName ?? model.File.FileName ?? string.Empty;
        var title = model.Title ??
                    CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Path.GetFileNameWithoutExtension(fileName));

        // Create document domain entity
        PaperlessDocument document;
        try
        {
            document = PaperlessDocument.New(s3Path, model.File.Length, DateTimeOffset.Now, new DocumentMetadata(fileName, title, model.Author));
        } catch (ValidationException ex)
        {
            logger.LogWarning("Upload failed validation: {errors}", string.Join(", ", ex.Errors));

            return TypedResults.UnprocessableEntity();
        }

        // Save document to repository
        await documentRepository.CreateAsync(document, ct);

        // Publish message to RabbitMQ
        var message = new DocumentUploadedMessage(document.Id, document.Path);
        messageQueueService.Publish(message); // Publishes the document info for OCR processing

        logger.LogInformation("Document {documentId} uploaded and message sent to queue.", document.Id);

        return TypedResults.CreatedAtRoute("GetDocumentById", new { id = document.Id.Value });
    }

    public static async Task<Results<Ok, NotFound<Guid>>> DeleteDocumentAsync([FromRoute] Guid id,
        IDocumentRepository documentRepository, ILoggerFactory loggerFactory, CancellationToken ct = default)
    {
        var logger = loggerFactory.CreateLogger(nameof(DocumentEndpoints));

        logger.LogInformation("Deleting document: {documentId}", id);

        var success = await documentRepository.DeleteAsync(new DocumentId(id), ct);
        if (!success)
            return TypedResults.NotFound<Guid>(id);

        return TypedResults.Ok();
    }
}
