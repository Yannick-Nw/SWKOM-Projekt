using Domain.Entities;
using System.Linq;
using Domain.Repositories;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApi.Models;
using System.Globalization;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using FluentValidation;
using System.Diagnostics.CodeAnalysis;
using Domain.Entities.Documents;
using System.IO;
using Application.Services.Documents;
using AutoMapper;
using Application.Interfaces;
using Domain.Messaging;

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

    public static async Task<Ok<IReadOnlyList<Document>>> GetDocumentsAsync(
        IDocumentService documentService, ILoggerFactory loggerFactory, CancellationToken ct = default)
    {
        var logger = loggerFactory.CreateLogger(nameof(DocumentEndpoints));
        logger.LogInformation("Fetching all documents");

        var documents = await documentService.GetAsync(ct);

        return TypedResults.Ok(documents);
    }

    public static async Task<Results<Ok<Document>, NotFound<Guid>>> GetDocumentAsync([FromRoute] Guid id,
        IDocumentService documentService, ILoggerFactory loggerFactory, CancellationToken ct = default)
    {
        var logger = loggerFactory.CreateLogger(nameof(DocumentEndpoints));
        logger.LogInformation("Fetching document: {documentId}", id);

        var document = await documentService.GetAsync(new DocumentId(id), ct);
        if (document is null)
            return TypedResults.NotFound<Guid>(id);

        return TypedResults.Ok(document);
    }

    public static async Task<Results<CreatedAtRoute, UnprocessableEntity>> UploadDocumentAsync([FromForm] UploadDocumentModel model,
        IDocumentService documentService, IMapper mapper, ILoggerFactory loggerFactory,
        CancellationToken ct = default)
    {
        // Logging
        var logger = loggerFactory.CreateLogger(nameof(DocumentEndpoints));
        logger.LogInformation("Uploading document: {model}", model);

        // Get file info
        var title = model.Title ??
                    CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Path.GetFileNameWithoutExtension(model.File.FileName));
        var file = mapper.Map<IFile>(model.File);

        // Create document domain entity
        Document document = Document.New(DateTimeOffset.Now, new DocumentMetadata(title, model.Author));

        // Run validation
        var validator = new DocumentValidator();
        var validationResult = validator.Validate(document);
        if (!validationResult.IsValid)
        {
            logger.LogWarning("Upload failed validation: {errors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            return TypedResults.UnprocessableEntity();
        }

        // Upload
        try
        {
            await documentService.CreateAsync(document, file, ct);
        } catch (ApplicationException ex)
        {
            logger.LogError(ex, "Failed to upload document: {model}", model);
            return TypedResults.UnprocessableEntity();
        }

        return TypedResults.CreatedAtRoute("GetDocumentById", new { Id = document.Id.Value });
    }

    public static async Task<Results<Ok, NotFound<Guid>>> DeleteDocumentAsync([FromRoute] Guid id,
        IDocumentService documentService, ILoggerFactory loggerFactory, CancellationToken ct = default)
    {
        // Logging
        var logger = loggerFactory.CreateLogger(nameof(DocumentEndpoints));
        logger.LogInformation("Deleting document: {documentId}", id);

        // Delete
        var success = await documentService.DeleteAsync(new DocumentId(id), ct);
        if (!success)
            return TypedResults.NotFound<Guid>(id);

        return TypedResults.Ok();
    }
}
