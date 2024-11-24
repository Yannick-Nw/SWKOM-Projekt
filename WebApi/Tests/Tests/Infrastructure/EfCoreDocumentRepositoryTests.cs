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

namespace Tests.Tests.Infrastructure;


public class EfCoreDocumentRepositoryTests
{
    [Fact]
    public async Task AddDocument_ShouldAddDocumentToDatabase()
    {
        // Arrange
        var sut = GetDocumentRepository();
        Document document = Document.New(
            DateTimeOffset.UtcNow,
            new DocumentMetadata("title", "author")
        );

        // Act
        await sut.CreateAsync(document);

        // Assert
        var fetchedDocument = await sut.GetAsync(document.Id);
        Assert.NotNull(fetchedDocument);
        Assert.Equal(document.Id, fetchedDocument.Id);
        Assert.Equal(document.UploadTime, fetchedDocument.UploadTime);
        Assert.Equal(document.Metadata, fetchedDocument.Metadata);
    }

    [Fact]
    public async Task UpdateDocument_ShouldUpdateDocumentInDatabase()
    {
        // Arrange
        var sut = GetDocumentRepository();
        Document document = Document.New(
            DateTimeOffset.UtcNow,
            new DocumentMetadata("title", "author")
        );
        var newMetadata = new DocumentMetadata("newTitle", "newAuthor");
        await sut.CreateAsync(document);

        // Act
        document.SetMetadata(newMetadata);
        var updateResult = await sut.UpdateAsync(document);

        // Assert
        Assert.True(updateResult);
        var updatedDocument = await sut.GetAsync(document.Id);
        Assert.NotNull(updatedDocument);
        Assert.Equal(newMetadata, updatedDocument.Metadata);
    }

    [Fact]
    public async Task DeleteDocument_ShouldRemoveDocumentFromDatabase()
    {
        // Arrange
        var sut = GetDocumentRepository();
        Document document = Document.New(
            DateTimeOffset.UtcNow,
            new DocumentMetadata("title", "author")
        );
        await sut.CreateAsync(document);

        // Act
        var deleteResult = await sut.DeleteAsync(document.Id);

        // Assert
        Assert.True(deleteResult);
        var deletedDocument = await sut.GetAsync(document.Id);
        Assert.Null(deletedDocument);
    }

    // Test GetAll
    [Fact]
    public async Task GetAllDocuments_ShouldReturnAllDocuments()
    {
        // Arrange
        var sut = GetDocumentRepository();
        var document1 = Document.New(
            DateTimeOffset.UtcNow,
            new DocumentMetadata("title1", "author1")
        );
        var document2 = Document.New(
            DateTimeOffset.UtcNow,
            new DocumentMetadata("title2", "author2")
        );
        await sut.CreateAsync(document1);
        await sut.CreateAsync(document2);

        // Act
        var documents = await sut.GetAsync();

        // Assert
        Assert.NotNull(documents);
        Assert.Equal(2, documents.Count);
        Assert.Contains(documents, d => d == document1);
        Assert.Contains(documents, d => d == document2);
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
