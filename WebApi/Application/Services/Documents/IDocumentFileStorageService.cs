using Application.Interfaces;
using Domain.Entities.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Documents;



/// <summary>
/// Implemented by Infrastructure layer to provide file storage for documents
/// </summary>
public interface IDocumentFileStorageService
{
    /// <summary>
    /// Retrieve file from storage
    /// </summary>
    Task<DocumentFile?> GetAsync(DocumentId id, CancellationToken ct = default);

    /// <summary>
    /// Upload file to storage
    /// </summary>
    Task UploadAsync(DocumentFile file, CancellationToken ct = default);

    /// <summary>
    /// Delete file from storage
    /// </summary>
    Task<bool> DeleteAsync(DocumentId id, CancellationToken ct = default);
}
