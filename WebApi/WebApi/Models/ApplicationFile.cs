using Application.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace WebApi.Models;

[ExcludeFromCodeCoverage]
public class ApplicationFile(IFormFile formFile) : IFile
{
    public string Name => formFile.FileName;
    public string ContentType => formFile.ContentType;

    public Task<Stream> OpenAsync() => Task.FromResult(formFile.OpenReadStream());

    public void Dispose() { }
}
