using Amazon.S3;
using Amazon.S3.Model;
using Application.Interfaces;
using Application.Services.Documents;
using Domain.Entities.Documents;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.FileStorage
{
    public class MinIOFileStorageService : IDocumentFileStorageService
    {
        private readonly string _bucketName;
        private readonly AmazonS3Client _s3Client;

        public MinIOFileStorageService(AmazonS3Client s3Client, MinIOConfiguration config)
        {
            _s3Client = s3Client;
            _bucketName = config.BucketName;
        }

        public async Task UploadAsync(DocumentFile file, CancellationToken ct = default)
        {
            using var fileStream = await file.File.OpenAsync();
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = file.Id.ToString(), // Use DocumentId as the key
                InputStream = fileStream,
                ContentType = file.File.ContentType // Use IFile's MIME type
            };

            await _s3Client.PutObjectAsync(putRequest, ct);
        }

        public async Task<DocumentFile?> GetAsync(DocumentId id, CancellationToken ct = default)
        {
            try
            {
                var getRequest = new GetObjectRequest { BucketName = _bucketName, Key = id.ToString() };

                using var response = await _s3Client.GetObjectAsync(getRequest, ct);
                // Wrap the response content in an implementation of IFile
                var file = new MinIOFile
                {
                    Name = id.ToString(),
                    ContentType = response.Headers["Content-Type"],
                    Length = response.ContentLength,
                    StreamProvider = async () => await Task.FromResult(response.ResponseStream)
                };

                return new DocumentFile(id, file); // Create and return a DocumentFile
            } catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null; // File not found
            }
        }

        public async Task<bool> DeleteAsync(DocumentId id, CancellationToken ct = default)
        {
            try
            {
                var deleteRequest = new DeleteObjectRequest { BucketName = _bucketName, Key = id.ToString() };

                await _s3Client.DeleteObjectAsync(deleteRequest, ct);
                return true;
            } catch
            {
                return false;
            }
        }

        public class MinIOFile : IFile
        {
            public Func<Task<Stream>> StreamProvider { get; set; }
            public string Name { get; set; } = string.Empty;
            public string ContentType { get; set; } = string.Empty;
            public long Length { get; set; }

            public async Task<Stream> OpenAsync() => await StreamProvider();
        }
    }
}