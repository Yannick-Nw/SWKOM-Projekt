using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Repositories;

public interface IDocumentRepository
{
    Task<IReadOnlyList<PaperlessDocument>> GetAsync(CancellationToken ct = default);
    Task<PaperlessDocument?> GetAsync(DocumentId id, CancellationToken ct = default);

    Task CreateAsync(PaperlessDocument document, CancellationToken ct = default);
    Task<bool> UpdateAsync(PaperlessDocument document, CancellationToken ct = default);
    Task<bool> DeleteAsync(DocumentId id, CancellationToken ct = default);
}
