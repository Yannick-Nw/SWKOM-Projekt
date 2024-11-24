using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Documents;

public class Document
{
    public DocumentId Id { get; }
    public DateTimeOffset UploadTime { get; }
    public DocumentMetadata Metadata { get; private set; }

    internal Document(DocumentId id, DateTimeOffset uploadTime, DocumentMetadata metadata)
    {
        Id = id;
        UploadTime = uploadTime;
        Metadata = metadata;
    }


    /// <summary>Create a new document</summary>
    /// <exception cref="ValidationException">Validation failed.</exception>
    public static Document New(DateTimeOffset uploadTime, DocumentMetadata metadata)
    {
        var document = new Document(DocumentId.New(), uploadTime, metadata);

        // Validate
        var validator = new DocumentValidator();
        validator.Validate(document);

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
