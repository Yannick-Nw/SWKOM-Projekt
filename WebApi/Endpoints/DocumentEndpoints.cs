using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApi.Endpoints;


public static class DocumentEndpoints
{
    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("document");

        group.MapGet("search", SearchDocumentAsync);

        group.MapPost("", UploadDocumentAsync);
        group.MapDelete("", DeleteDocumentAsync);

        group.MapGet("metadata", GetDocumentMetadataAsync);
        group.MapDelete("metadata", DeleteDocumentMetadataAsync);

        return builder;
    }

    private static Task SearchDocumentAsync(ILogger logger)
    {
        logger.LogInformation("Searching document...");

        return Task.CompletedTask;
    }

    #region Document
    private static Task UploadDocumentAsync(IFormFile file, ILogger logger)
    {
        logger.LogInformation("Uploading document...");

        return Task.CompletedTask;
    }
    private static Task DeleteDocumentAsync(ILogger logger)
    {
        logger.LogInformation("Deleting document...");

        return Task.CompletedTask;
    }
    #endregion

    #region Metadata
    private static Task GetDocumentMetadataAsync(ILogger logger)
    {
        logger.LogInformation("Get document metadata...");

        return Task.CompletedTask;
    }

    private static Task DeleteDocumentMetadataAsync(ILogger logger)
    {
        logger.LogInformation("Delete document metadata...");

        return Task.CompletedTask;
    }
    #endregion
}
