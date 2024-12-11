using Domain.Entities.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Repositories.Documents;

public interface IDocumentRepository
{
    Task<IReadOnlyList<Document>> GetAsync(CancellationToken ct = default);
    Task<Document?> GetAsync(DocumentId id, CancellationToken ct = default);

    Task CreateAsync(Document document, CancellationToken ct = default);
    Task<bool> UpdateAsync(Document document, CancellationToken ct = default);
    Task<bool> DeleteAsync(DocumentId id, CancellationToken ct = default);
}
