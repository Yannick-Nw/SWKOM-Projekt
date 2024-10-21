using Domain.Entities;
using System.Linq;
using Domain.Repositories;
using Infrastructure.Repositories.EntityFrameworkCore;
using Infrastructure.Repositories.EntityFrameworkCore.Dbos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApi.Models;
using System.Globalization;

namespace WebApi.Endpoints;


public static class DocumentEndpoints
{
    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/document");

        group.MapGet("", GetDocumentsAsync);
        group
            .MapGet("{id:guid}", GetDocumentAsync)
            .WithName("GetDocumentById");

        //group.MapGet("search", SearchDocumentAsync);

        group
            .MapPost("", UploadDocumentAsync)
            .DisableAntiforgery();
        group.MapDelete("{id:guid}", DeleteDocumentAsync);

        return builder;
    }

    public static async Task<Ok<IReadOnlyList<PaperlessDocument>>> GetDocumentsAsync(IDocumentRepository documentRepository, ILogger logger, CancellationToken ct = default)
    {
        logger.LogInformation("Fetching documents...");

        var documents = await documentRepository.GetAsync(ct);

        return TypedResults.Ok(documents);
    }
    public static async Task<Results<Ok<PaperlessDocument>, NotFound<Guid>>> GetDocumentAsync([FromRoute] Guid id, IDocumentRepository documentRepository, ILogger logger, CancellationToken ct = default)
    {
        logger.LogInformation("Fetching document: {documentId}", id);

        var document = await documentRepository.GetAsync(new DocumentId(id), ct);
        if (document is null)
            return TypedResults.NotFound<Guid>(id);

        return TypedResults.Ok(document);
    }


    //public static Task SearchDocumentAsync(ILogger logger)
    //{
    //    logger.LogInformation("Searching document...");

    //    return Task.CompletedTask;
    //}

    #region Document
    public static async Task<CreatedAtRoute> UploadDocumentAsync([FromForm] UploadDocumentModel model, IDocumentRepository documentRepository, ILogger logger, CancellationToken ct = default)
    {
        logger.LogInformation("Uploading document: {model}", model);

        var id = DocumentId.New();

        // Upload to S3
        var s3Path = Guid.NewGuid().ToString(); // Mocked

        var fileName = model.FileName ?? model.File.FileName;
        var title = model.Title ?? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Path.GetFileNameWithoutExtension(fileName));
        var document = new PaperlessDocument(id, s3Path, model.File.Length, DateTimeOffset.Now, new DocumentMetadata(fileName, title, model.Author));

        await documentRepository.CreateAsync(document, ct);

        return TypedResults.CreatedAtRoute("GetDocumentById", new { id = id.Value });
    }
    public static async Task<Results<Ok, NotFound<Guid>>> DeleteDocumentAsync([FromRoute] Guid id, IDocumentRepository documentRepository, ILogger logger, CancellationToken ct = default)
    {
        logger.LogInformation("Deleting document: {documentId}", id);

        var success = await documentRepository.DeleteAsync(new DocumentId(id), ct);
        if (!success)
            return TypedResults.NotFound<Guid>(id);

        return TypedResults.Ok();
    }
    #endregion
}
