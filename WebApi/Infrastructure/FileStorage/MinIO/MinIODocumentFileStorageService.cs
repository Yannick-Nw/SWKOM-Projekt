using Amazon.S3.Model;
using Amazon.S3;
using Application.Interfaces;
using Application.Services.Documents;
using Domain.Entities.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using Application.Interfaces.Files;

namespace Infrastructure.FileStorage.MinIO;

public record MinIOFile(GetObjectResponse Response) : IFile
{
    public string ContentType => Response.Headers.ContentType;

    public Task<Stream> OpenAsync() => Task.FromResult(Response.ResponseStream);

    public void Dispose() => Response.Dispose();
}
public class MinIODocumentFileStorageService(AmazonS3Client s3Client, ILogger<MinIODocumentFileStorageService> logger) : IDocumentFileStorageService
{
    public const string BUCKET_NAME = "documents";

    public async Task UploadAsync(DocumentFile file, CancellationToken ct = default)
    {
        using var fileStream = await file.File.OpenAsync();

        var putRequest = new PutObjectRequest
        {
            BucketName = BUCKET_NAME,
            Key = file.Id.ToString(),
            InputStream = fileStream,
            ContentType = file.File.ContentType,
        };

        await s3Client.PutObjectAsync(putRequest, ct);

        logger.LogInformation("Uploaded document \"{documentId}\" to MinIO.", file.Id.ToString());
    }

    public async Task<DocumentFile?> GetAsync(DocumentId id, CancellationToken ct = default)
    {
        try
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = BUCKET_NAME,
                Key = id.ToString()
            };

            var response = await s3Client.GetObjectAsync(getRequest, ct);
            var file = new MinIOFile(response);

            return new DocumentFile(id, file);
        } catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogWarning("Document \"{documentId}\" not found in MinIO.", id);

            return null;
        }
    }

    public async Task<bool> DeleteAsync(DocumentId id, CancellationToken ct = default)
    {
        try
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = BUCKET_NAME,
                Key = id.ToString()
            };

            var response = await s3Client.DeleteObjectAsync(deleteRequest, ct);
            var success = response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
            if (!success)
                return false;

            logger.LogInformation("Deleted document \"{documentId}\" from MinIO.", id);

            return true;
        } catch
        {
            return false;
        }
    }


    public void Dispose()
    {

    }
}
