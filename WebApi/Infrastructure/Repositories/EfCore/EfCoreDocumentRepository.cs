using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities.Documents;
using Domain.Repositories.Documents;
using Infrastructure.Repositories.EfCore.Dbos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.EfCore;

public class EfCoreDocumentRepository(PaperlessDbContext dbContext, IMapper mapper) : IDocumentRepository
{
    public async Task<IReadOnlyList<Document>> GetAsync(CancellationToken ct = default)
    {
        var dbos = await dbContext.Documents.ToListAsync(ct);

        return mapper.Map<IReadOnlyList<Document>>(dbos);
    }
    public async Task<Document?> GetAsync(DocumentId id, CancellationToken ct = default)
    {
        var dbo = await dbContext.Documents.FindAsync([id.Value], cancellationToken: ct);

        return mapper.Map<Document>(dbo);
    }

    public async Task CreateAsync(Document document, CancellationToken ct = default)
    {
        var dbo = mapper.Map<DocumentDbo>(document);

        dbContext.Documents.Add(dbo);

        await dbContext.SaveChangesAsync(ct);
    }
    public async Task<bool> UpdateAsync(Document document, CancellationToken ct = default)
    {
        var dbo = await dbContext.Documents.FindAsync([document.Id.Value], cancellationToken: ct);
        if (dbo is null) return false;

        mapper.Map(document, dbo);
        dbContext.Documents.Update(dbo);

        return await dbContext.SaveChangesAsync(ct) > 0;
    }
    public async Task<bool> DeleteAsync(DocumentId id, CancellationToken ct = default)
    {
        var dbo = await dbContext.Documents.FindAsync([id.Value], cancellationToken: ct);
        if (dbo is null) return false;

        dbContext.Documents.Remove(dbo);

        return await dbContext.SaveChangesAsync(ct) > 0;
    }
}
