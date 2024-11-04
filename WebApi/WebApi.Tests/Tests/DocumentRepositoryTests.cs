using AutoMapper;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Repositories.EntityFrameworkCore;
using Infrastructure.Repositories.EntityFrameworkCore.Repositories;
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
        PaperlessDocument document = PaperlessDocument.New(
            "path/to/document",
            12345,
            DateTimeOffset.UtcNow,
            new DocumentMetadata("fileName", "title", "author")
        );

        // Act
        await repository.CreateAsync(document);

        // Assert
        var fetchedDocument = await repository.GetAsync(document.Id);
        Assert.NotNull(fetchedDocument);
        Assert.Equal("path/to/document", fetchedDocument.Path);
    }

    [Fact]
    public async Task UpdateDocument_ShouldUpdateDocumentInDatabase()
    {
        // Arrange
        var repository = GetDocumentRepository();
        PaperlessDocument document = PaperlessDocument.New(
            "path/to/document",
            12345,
            DateTimeOffset.UtcNow,
            new DocumentMetadata("fileName", "title", "author")
        );
        await repository.CreateAsync(document);

        // Act
        document.SetMetadata(new DocumentMetadata("newFileName", "newTitle", "newAuthor"));
        var updateResult = await repository.UpdateAsync(document);

        // Assert
        Assert.True(updateResult);
        var updatedDocument = await repository.GetAsync(document.Id);
        Assert.NotNull(updatedDocument);
        Assert.Equal("newFileName", updatedDocument.Metadata.FileName);
        Assert.Equal("newTitle", updatedDocument.Metadata.Title);
        Assert.Equal("newAuthor", updatedDocument.Metadata.Author);
    }

    [Fact]
    public async Task DeleteDocument_ShouldRemoveDocumentFromDatabase()
    {
        // Arrange
        var repository = GetDocumentRepository();
        PaperlessDocument document = PaperlessDocument.New(
            "path/to/document",
            12345,
            DateTimeOffset.UtcNow,
            new DocumentMetadata("fileName", "title", "author")
        );
        await repository.CreateAsync(document);

        // Act
        var deleteResult = await repository.DeleteAsync(document.Id);

        // Assert
        Assert.True(deleteResult);
        var deletedDocument = await repository.GetAsync(document.Id);
        Assert.Null(deletedDocument);
    }

    private static DocumentRepository GetDocumentRepository()
    {
        return new DocumentRepository(GetPaperlessDbContext(), GetMapper());
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
