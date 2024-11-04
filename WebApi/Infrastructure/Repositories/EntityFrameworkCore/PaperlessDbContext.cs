using Domain.Entities;
using Infrastructure.Repositories.EntityFrameworkCore.Dbos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Repositories.EntityFrameworkCore;

public class PaperlessDbContext(DbContextOptions<PaperlessDbContext> options) : DbContext(options)
{
    public DbSet<PaperlessDocumentDbo> Documents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<PaperlessDocumentDbo>()
            .ToTable("Document");

        base.OnModelCreating(builder);
    }
}