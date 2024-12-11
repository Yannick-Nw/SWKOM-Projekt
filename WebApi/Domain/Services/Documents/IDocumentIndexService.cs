using Domain.Entities.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.Documents;
public interface IDocumentIndexService : IDisposable
{
    Task<bool> StoreAsync(DocumentId id, string content, CancellationToken ct = default);
    Task<bool> DeleteAsync(DocumentId id, CancellationToken ct = default);
    Task<string?> GetAsync(DocumentId id, CancellationToken ct = default);

    Task<IReadOnlyList<DocumentId>> SearchAsync(string query, CancellationToken ct = default);
}
