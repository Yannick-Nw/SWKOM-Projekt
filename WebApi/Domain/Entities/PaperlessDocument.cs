using Domain.Validation;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;
public class PaperlessDocument
{
    public DocumentId Id { get; }
    public string Path { get; }
    public long Size { get; }
    public DateTimeOffset UploadTime { get; }
    public DocumentMetadata Metadata { get; private set; }

    public PaperlessDocument(DocumentId id, string path, long size, DateTimeOffset uploadTime, DocumentMetadata metadata)
    {
        Id = id;
        Path = path;
        Size = size;
        UploadTime = uploadTime;
        Metadata = metadata;
    }


    /// <summary>Create a new document</summary>
    /// <exception cref="ValidationException">Validation failed.</exception>
    public static PaperlessDocument New(string path, long size, DateTimeOffset uploadTime, DocumentMetadata metadata)
    {
        var document = new PaperlessDocument(DocumentId.New(), path, size, uploadTime, metadata);

        // Validate
        var validator = new PaperlessDocumentValidator();
        validator.ValidateAndThrow(document);

        return document;
    }

    /// <summary>Update document metadata</summary>
    /// <exception cref="ValidationException">Validation failed.</exception>
    public void SetMetadata(DocumentMetadata metadata)
    {
        // Validate
        var validator = new DocumentMetadataValidator();
        validator.ValidateAndThrow(metadata);

        Metadata = metadata;
    }
}
