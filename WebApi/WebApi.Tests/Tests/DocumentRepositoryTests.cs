using AutoMapper;
using Domain.Entities;
using Domain.Entities.Documents;
using Infrastructure;
using Infrastructure.Repositories.EfCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.Tests.Tests;


public class DocumentRepositoryTests
{
    [Fact]
    public async Task AddDocument_ShouldAddDocumentToDatabase()
    {
        // Arrange
        var repository = GetDocumentRepository();
        Document document = Document.New(
            DateTimeOffset.UtcNow,
            new DocumentMetadata("title", "author")
        );

        // Act
        await repository.CreateAsync(document);

        // Assert
        var fetchedDocument = await repository.GetAsync(document.Id);
        Assert.NotNull(fetchedDocument);
        Assert.Equal(document.Id, fetchedDocument.Id);
        Assert.Equal(document.UploadTime, fetchedDocument.UploadTime);
        Assert.Equal(document.Metadata, fetchedDocument.Metadata);
    }

    [Fact]
    public async Task UpdateDocument_ShouldUpdateDocumentInDatabase()
    {
        // Arrange
        var repository = GetDocumentRepository();
        Document document = Document.New(
            DateTimeOffset.UtcNow,
            new DocumentMetadata("title", "author")
        );
        var newMetadata = new DocumentMetadata("newTitle", "newAuthor");
        await repository.CreateAsync(document);

        // Act
        document.SetMetadata(newMetadata);
        var updateResult = await repository.UpdateAsync(document);

        // Assert
        Assert.True(updateResult);
        var updatedDocument = await repository.GetAsync(document.Id);
        Assert.NotNull(updatedDocument);
        Assert.Equal(newMetadata, updatedDocument.Metadata);
    }

    [Fact]
    public async Task DeleteDocument_ShouldRemoveDocumentFromDatabase()
    {
        // Arrange
        var repository = GetDocumentRepository();
        Document document = Document.New(
            DateTimeOffset.UtcNow,
            new DocumentMetadata("title", "author")
        );
        await repository.CreateAsync(document);

        // Act
        var deleteResult = await repository.DeleteAsync(document.Id);

        // Assert
        Assert.True(deleteResult);
        var deletedDocument = await repository.GetAsync(document.Id);
        Assert.Null(deletedDocument);
    }

    private static EfCoreDocumentRepository GetDocumentRepository()
    {
        return new EfCoreDocumentRepository(GetPaperlessDbContext(), GetMapper());
    }

    private static PaperlessDbContext GetPaperlessDbContext()
    {
        var options = new DbContextOptionsBuilder<PaperlessDbContext>()
            .UseInMemoryDatabase(databaseName: "InMemoryDb")
            .Options;

        // Clear database
        using (var context = new PaperlessDbContext(options))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        return new PaperlessDbContext(options);
    }

    private static IMapper GetMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<InfrastructureProfile>();
        });
        return config.CreateMapper();
    }
}
