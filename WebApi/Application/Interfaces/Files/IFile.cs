namespace Application.Interfaces.Files;

public interface IFile : IDisposable
{
    string ContentType { get; }

    Task<Stream> OpenAsync();
}
