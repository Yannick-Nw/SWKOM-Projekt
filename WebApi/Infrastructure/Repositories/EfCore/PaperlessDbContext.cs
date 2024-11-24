using Domain.Entities;
using Infrastructure.Repositories.EfCore.Dbos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Repositories.EfCore;

public class PaperlessDbContext(DbContextOptions<PaperlessDbContext> options) : DbContext(options)
{
    public DbSet<DocumentDbo> Documents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<DocumentDbo>()
            .ToTable("Document");

        base.OnModelCreating(builder);
    }
}