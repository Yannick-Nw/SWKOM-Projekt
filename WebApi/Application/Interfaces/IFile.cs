namespace Application.Interfaces;

public interface IFile : IDisposable
{
    string Name { get; }
    string ContentType { get; }

    Task<Stream> OpenAsync();
}
