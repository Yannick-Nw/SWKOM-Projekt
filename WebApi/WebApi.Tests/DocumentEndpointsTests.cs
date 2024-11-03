using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using WebApi.Endpoints;
using WebApi.Models;
using WebApi.Services.Messaging;
using Xunit;

namespace WebApi.Tests;

public class DocumentEndpointsTests
{
    private readonly Mock<IDocumentRepository> _mockRepository;
    private readonly Mock<IMessageQueueService> _mockMessageQueue;
    private readonly Mock<ILogger> _mockLogger;

    public DocumentEndpointsTests()
    {
        _mockRepository = new Mock<IDocumentRepository>();
        _mockMessageQueue = new Mock<IMessageQueueService>();
        _mockLogger = new Mock<ILogger>();
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldReturnDocuments_WhenDocumentsExist()
    {
        // Arrange
        var documents = new List<PaperlessDocument>
        {
            new PaperlessDocument(DocumentId.New(), "s3Path1", 1024, DateTimeOffset.Now, new DocumentMetadata("file1.pdf", "Title 1", "Author 1")),
            new PaperlessDocument(DocumentId.New(), "s3Path2", 2048, DateTimeOffset.Now, new DocumentMetadata("file2.pdf", "Title 2", "Author 2"))
        };
        _mockRepository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(documents);

        // Act
        var result = await DocumentEndpoints.GetDocumentsAsync(_mockRepository.Object, _mockLogger.Object);

        // Assert
        Assert.IsType<Ok<IReadOnlyList<PaperlessDocument>>>(result);
        Assert.Equal(documents, result.Value);
    }

    [Fact]
    public async Task GetDocumentAsync_ShouldReturnDocument_WhenDocumentExists()
    {
        // Arrange
        var documentId = DocumentId.New();
        var document = new PaperlessDocument(documentId, "s3Path", 1024, DateTimeOffset.Now, new DocumentMetadata("file.pdf", "Title", "Author"));
        _mockRepository.Setup(r => r.GetAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        // Act
        var result = await DocumentEndpoints.GetDocumentAsync(documentId.Value, _mockRepository.Object, _mockLogger.Object);
        
        // Assert
        Assert.IsType<Ok<PaperlessDocument>>(result.Result);
        Assert.Equal(document, ((Ok<PaperlessDocument>)result.Result).Value);
    }

    [Fact]
    public async Task GetDocumentAsync_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        // Arrange
        var documentId = DocumentId.New();
        _mockRepository.Setup(r => r.GetAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync((PaperlessDocument)null);

        // Act
        var result = await DocumentEndpoints.GetDocumentAsync(documentId.Value, _mockRepository.Object, _mockLogger.Object);

        // Assert
        Assert.IsType<NotFound<Guid>>(result.Result);
        Assert.Equal(documentId.Value, ((NotFound<Guid>)result.Result).Value);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ShouldReturnOk_WhenDocumentIsDeleted()
    {
        // Arrange
        var documentId = DocumentId.New();
        _mockRepository.Setup(r => r.DeleteAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await DocumentEndpoints.DeleteDocumentAsync(documentId.Value, _mockRepository.Object, _mockLogger.Object);

        // Assert
        Assert.IsType<Ok>(result.Result);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        // Arrange
        var documentId = DocumentId.New();
        _mockRepository.Setup(r => r.DeleteAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await DocumentEndpoints.DeleteDocumentAsync(documentId.Value, _mockRepository.Object, _mockLogger.Object);

        // Assert
        Assert.IsType<NotFound<Guid>>(result.Result);
        Assert.Equal(documentId.Value, ((NotFound<Guid>)result.Result).Value);
    }

    [Fact]
    public async Task UploadDocumentAsync_ShouldReturnCreatedAtRoute()
    {
        // Arrange
        var file = new Mock<IFormFile>();
        file.Setup(f => f.FileName).Returns("file.pdf");
        file.Setup(f => f.Length).Returns(1024);

        var model = new UploadDocumentModel(file.Object)
        {
            FileName = "file.pdf",
            Title = "Title",
            Author = "Author"
        };
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<PaperlessDocument>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await DocumentEndpoints.UploadDocumentAsync(model, _mockRepository.Object, _mockMessageQueue.Object, _mockLogger.Object);

        // Assert
        Assert.IsType<CreatedAtRoute>(result.Result);
        Assert.Equal("GetDocumentById", ((CreatedAtRoute)result.Result).RouteName);
    }

    [Fact]
    public async Task UploadDocumentAsync_ShouldReturnUnprocessableEntity_WhenModelIsInvalid()
    {
        // Arrange
        var model = new UploadDocumentModel(new Mock<IFormFile>().Object);
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<PaperlessDocument>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await DocumentEndpoints.UploadDocumentAsync(model, _mockRepository.Object, _mockMessageQueue.Object, _mockLogger.Object);

        // Assert
        Assert.IsType<UnprocessableEntity>(result.Result);
    }
}
