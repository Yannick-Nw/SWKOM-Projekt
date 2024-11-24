using Application.Interfaces;

namespace WebApi.Models;

public class ApplicationFile(IFormFile formFile) : IFile
{
    public string Name => formFile.Name;
    public string ContentType => formFile.ContentType;
    public long Length => formFile.Length;

    public Task<Stream> OpenAsync() => Task.FromResult(formFile.OpenReadStream());
}
