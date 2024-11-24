using Application.Interfaces;
using Application.Services.Documents;
using Domain.Entities.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.FileStorage;
public class MinIOFileStorageService : IDocumentFileStorageService
{
    public Task<bool> DeleteAsync(DocumentId id, CancellationToken ct = default)
    {
        return Task.FromResult(true);
        throw new NotImplementedException();
    }

    public Task<DocumentFile?> GetAsync(DocumentId id, CancellationToken ct = default)
    {
        return Task.FromResult(null as DocumentFile);
        throw new NotImplementedException();
    }

    public Task UploadAsync(DocumentFile file, CancellationToken ct = default)
    {
        return Task.CompletedTask;
        throw new NotImplementedException();
    }
}
