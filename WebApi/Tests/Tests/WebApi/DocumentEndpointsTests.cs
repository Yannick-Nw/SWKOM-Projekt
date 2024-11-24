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

namespace Tests.Tests.WebApi;

public class DocumentEndpointsTests
{
    [Fact]
    public async Task GetDocumentsAsync_ShouldReturnDocuments_WhenDocumentsExist()
    {
        // Arrange
        var mockService = new Mock<IDocumentService>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var documents = new List<Document>
        {
            new Document(DocumentId.New(), DateTimeOffset.Now, new DocumentMetadata("Title 1", "Author 1")),
            new Document(DocumentId.New(), DateTimeOffset.Now, new DocumentMetadata("Title 2", "Author 2"))
        };
        mockService.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(documents);

        // Act
        var result = await DocumentEndpoints.GetDocumentsAsync(mockService.Object, mockLoggerFactory.Object);

        // Assert
        Assert.IsType<Ok<IReadOnlyList<Document>>>(result);
        Assert.Equal(documents, result.Value);
    }

    [Fact]
    public async Task GetDocumentAsync_ShouldReturnDocument_WhenDocumentExists()
    {
        // Arrange
        var mockService = new Mock<IDocumentService>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var documentId = DocumentId.New();
        var document = new Document(documentId, DateTimeOffset.Now, new DocumentMetadata("Title", "Author"));
        mockService.Setup(r => r.GetAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        // Act
        var result = await DocumentEndpoints.GetDocumentAsync(documentId.Value, mockService.Object, mockLoggerFactory.Object);

        // Assert
        Assert.IsType<Ok<Document>>(result.Result);
        Assert.Equal(document, ((Ok<Document>)result.Result).Value);
    }

    [Fact]
    public async Task GetDocumentAsync_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        // Arrange
        var mockService = new Mock<IDocumentService>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var documentId = DocumentId.New();
        mockService.Setup(r => r.GetAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(null as Document);

        // Act
        var result = await DocumentEndpoints.GetDocumentAsync(documentId.Value, mockService.Object, mockLoggerFactory.Object);

        // Assert
        Assert.IsType<NotFound<Guid>>(result.Result);
        Assert.Equal(documentId.Value, ((NotFound<Guid>)result.Result).Value);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ShouldReturnOk_WhenDocumentIsDeleted()
    {
        // Arrange
        var mockService = new Mock<IDocumentService>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var documentId = DocumentId.New();
        mockService.Setup(r => r.DeleteAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await DocumentEndpoints.DeleteDocumentAsync(documentId.Value, mockService.Object, mockLoggerFactory.Object);

        // Assert
        Assert.IsType<Ok>(result.Result);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        // Arrange
        var mockService = new Mock<IDocumentService>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var documentId = DocumentId.New();
        mockService.Setup(r => r.DeleteAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await DocumentEndpoints.DeleteDocumentAsync(documentId.Value, mockService.Object, mockLoggerFactory.Object);

        // Assert
        Assert.IsType<NotFound<Guid>>(result.Result);
        Assert.Equal(documentId.Value, ((NotFound<Guid>)result.Result).Value);
    }

    [Fact]
    public async Task UploadDocumentAsync_ShouldReturnCreatedAtRoute()
    {
        // Arrange
        var mockService = new Mock<IDocumentService>();
        var mockMapper = new Mock<IMapper>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("file.pdf");
        mockFile.Setup(f => f.Length).Returns(1024);

        var model = new UploadDocumentModel(mockFile.Object)
        {
            FileName = "file.pdf",
            Title = "Title",
            Author = "Author"
        };
        mockService.Setup(r => r.CreateAsync(It.IsAny<Document>(), It.IsAny<IFile>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await DocumentEndpoints.UploadDocumentAsync(model, mockService.Object, mockMapper.Object, mockLoggerFactory.Object);

        // Assert
        Assert.IsType<CreatedAtRoute>(result.Result);
        Assert.Equal("GetDocumentById", ((CreatedAtRoute)result.Result).RouteName);
    }

    [Fact]
    public async Task UploadDocumentAsync_ShouldReturnUnprocessableEntity_WhenModelIsInvalid()
    {
        // Arrange
        var mockService = new Mock<IDocumentService>();
        var mockMapper = new Mock<IMapper>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        mockLoggerFactory.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var model = new UploadDocumentModel(new Mock<IFormFile>().Object);
        mockService.Setup(r => r.CreateAsync(It.IsAny<Document>(), It.IsAny<IFile>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await DocumentEndpoints.UploadDocumentAsync(model, mockService.Object, mockMapper.Object, mockLoggerFactory.Object);

        // Assert
        Assert.IsType<UnprocessableEntity>(result.Result);
    }
}
