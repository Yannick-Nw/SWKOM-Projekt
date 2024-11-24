namespace Application.Interfaces;

public interface IFile
{
    string Name { get; }
    string ContentType { get; }
    long Length { get; }

    Task<Stream> OpenAsync();
}
