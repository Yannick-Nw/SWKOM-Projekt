using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Repositories.EntityFrameworkCore.Dbos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.EntityFrameworkCore.Repositories;

public class DocumentRepository(PaperlessDbContext dbContext, IMapper mapper) : IDocumentRepository
{
    public async Task<IReadOnlyList<PaperlessDocument>> GetAsync(CancellationToken ct = default)
    {
        var dbos = await dbContext.Documents.ToListAsync(ct);

        return mapper.Map<IReadOnlyList<PaperlessDocument>>(dbos);
    }
    public async Task<PaperlessDocument?> GetAsync(DocumentId id, CancellationToken ct = default)
    {
        var dbo = await dbContext.Documents.FindAsync([id.Value], cancellationToken: ct);

        return mapper.Map<PaperlessDocument>(dbo);
    }

    public async Task CreateAsync(PaperlessDocument document, CancellationToken ct = default)
    {
        var dbo = mapper.Map<PaperlessDocumentDbo>(document);

        dbContext.Documents.Add(dbo);

        await dbContext.SaveChangesAsync(ct);
    }
    public async Task<bool> UpdateAsync(PaperlessDocument document, CancellationToken ct = default)
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
