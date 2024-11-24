using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Services.Documents;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Documents;
using Domain.Messaging;
using Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using WebApi.Endpoints;
using WebApi.Models;
using Xunit;

namespace WebApi.Tests.Tests;

public class DocumentEndpointsTests
{
    private readonly Mock<IDocumentService> _mockService;
    private readonly Mock<ILoggerFactory> _mockLogger;
    private readonly Mock<IMapper> _mockMapper;

    public DocumentEndpointsTests()
    {
        _mockService = new Mock<IDocumentService>();
        _mockLogger = new Mock<ILoggerFactory>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldReturnDocuments_WhenDocumentsExist()
    {
        // Arrange
        var documents = new List<Document>
        {
            new Document(DocumentId.New(), DateTimeOffset.Now, new DocumentMetadata( "Title 1", "Author 1")),
            new Document(DocumentId.New(), DateTimeOffset.Now, new DocumentMetadata("Title 2", "Author 2"))
        };
        _mockService.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(documents);

        // Act
        var result = await DocumentEndpoints.GetDocumentsAsync(_mockService.Object, _mockLogger.Object);

        // Assert
        Assert.IsType<Ok<IReadOnlyList<Document>>>((object)result);
        Assert.Equal(documents, result.Value);
    }

    [Fact]
    public async Task GetDocumentAsync_ShouldReturnDocument_WhenDocumentExists()
    {
        // Arrange
        var documentId = DocumentId.New();
        var document = new Document(documentId, DateTimeOffset.Now, new DocumentMetadata("Title", "Author"));
        _mockService.Setup(r => r.GetAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        // Act
        var result = await DocumentEndpoints.GetDocumentAsync(documentId.Value, _mockService.Object, _mockLogger.Object);

        // Assert
        Assert.IsType<Ok<Document>>((object)result.Result);
        Assert.Equal(document, ((Ok<Document>)result.Result).Value);
    }

    [Fact]
    public async Task GetDocumentAsync_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        // Arrange
        var documentId = DocumentId.New();
        _mockService.Setup(r => r.GetAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(null as Document);

        // Act
        var result = await DocumentEndpoints.GetDocumentAsync(documentId.Value, _mockService.Object, _mockLogger.Object);

        // Assert
        Assert.IsType<NotFound<Guid>>(result.Result);
        Assert.Equal(documentId.Value, ((NotFound<Guid>)result.Result).Value);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ShouldReturnOk_WhenDocumentIsDeleted()
    {
        // Arrange
        var documentId = DocumentId.New();
        _mockService.Setup(r => r.DeleteAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await DocumentEndpoints.DeleteDocumentAsync(documentId.Value, _mockService.Object, _mockLogger.Object);

        // Assert
        Assert.IsType<Ok>(result.Result);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        // Arrange
        var documentId = DocumentId.New();
        _mockService.Setup(r => r.DeleteAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await DocumentEndpoints.DeleteDocumentAsync(documentId.Value, _mockService.Object, _mockLogger.Object);

        // Assert
        Assert.IsType<NotFound<Guid>>(result.Result);
        Assert.Equal(documentId.Value, ((NotFound<Guid>)result.Result).Value);
    }

    [Fact]
    public async Task UploadDocumentAsync_ShouldReturnCreatedAtRoute()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("file.pdf");
        mockFile.Setup(f => f.Length).Returns(1024);

        var model = new UploadDocumentModel(mockFile.Object)
        {
            FileName = "file.pdf",
            Title = "Title",
            Author = "Author"
        };
        _mockService.Setup(r => r.CreateAsync(It.IsAny<Document>(), It.IsAny<IFile>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await DocumentEndpoints.UploadDocumentAsync(model, _mockService.Object, _mockMapper.Object, _mockLogger.Object);

        // Assert
        Assert.IsType<CreatedAtRoute>(result.Result);
        Assert.Equal("GetDocumentById", ((CreatedAtRoute)result.Result).RouteName);
    }

    [Fact]
    public async Task UploadDocumentAsync_ShouldReturnUnprocessableEntity_WhenModelIsInvalid()
    {
        // Arrange
        var model = new UploadDocumentModel(new Mock<IFormFile>().Object);
        _mockService.Setup(r => r.CreateAsync(It.IsAny<Document>(), It.IsAny<IFile>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await DocumentEndpoints.UploadDocumentAsync(model, _mockService.Object, _mockMapper.Object, _mockLogger.Object);

        // Assert
        Assert.IsType<UnprocessableEntity>(result.Result);
    }
}
