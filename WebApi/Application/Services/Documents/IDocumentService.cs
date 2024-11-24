using Application.Interfaces;
using Domain.Entities.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Documents;

public interface IDocumentService
{
    Task<IReadOnlyList<Document>> GetAsync(CancellationToken ct = default);
    Task<Document?> GetAsync(DocumentId id, CancellationToken ct = default);

    /// <exception cref="ApplicationException">Creation failed</exception>
    Task CreateAsync(Document document, IFile file, CancellationToken ct = default);

    Task<bool> DeleteAsync(DocumentId id, CancellationToken ct = default);
}
