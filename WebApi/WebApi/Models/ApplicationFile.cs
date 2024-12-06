using Application.Interfaces;
using Application.Interfaces.Files;
using System.Diagnostics.CodeAnalysis;

namespace WebApi.Models;

[ExcludeFromCodeCoverage]
public class ApplicationFile(IFormFile formFile) : IFile
{
    public string ContentType => formFile.ContentType;

    public Task<Stream> OpenAsync() => Task.FromResult(formFile.OpenReadStream());

    public void Dispose() { }
}
