using Infrastructure.Repositories.EntityFrameworkCore.Dbos;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.EntityFrameworkCore;

public class PaperlessDbContext(DbContextOptions<PaperlessDbContext> options) : DbContext(options)
{
    public DbSet<TestDbo> Tests { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<TestDbo>()
            .HasData(new TestDbo { Id = 1, Name = "Test" }, new TestDbo { Id = 2, Name = "Test 2" });

        base.OnModelCreating(builder);
    }
}