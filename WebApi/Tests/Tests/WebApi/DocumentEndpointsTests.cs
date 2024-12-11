using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Interfaces.Files;
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
using OcrWorker.Services;
using WebApi.Endpoints;
using WebApi.Mappings;
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
            new Document(DocumentId.New(), DateTimeOffset.Now,
                new DocumentMetadata("file.pdf", "Title 1", "Author 1")),
            new Document(DocumentId.New(), DateTimeOffset.Now,
                new DocumentMetadata("file.pdf", "Title 2", "Author 2"))
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
        var document = new Document(documentId, DateTimeOffset.Now,
            new DocumentMetadata("file.pdf", "Title", "Author"));
        mockService.Setup(r => r.GetAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        // Act
        var result =
            await DocumentEndpoints.GetDocumentAsync(documentId.Value, mockService.Object, mockLoggerFactory.Object);

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
        var result =
            await DocumentEndpoints.GetDocumentAsync(documentId.Value, mockService.Object, mockLoggerFactory.Object);

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
        var result =
            await DocumentEndpoints.DeleteDocumentAsync(documentId.Value, mockService.Object, mockLoggerFactory.Object);

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
        var result =
            await DocumentEndpoints.DeleteDocumentAsync(documentId.Value, mockService.Object, mockLoggerFactory.Object);

        // Assert
        Assert.IsType<NotFound<Guid>>(result.Result);
        Assert.Equal(documentId.Value, ((NotFound<Guid>)result.Result).Value);
    }

    [Fact]
    public async Task UploadDocumentAsync_ShouldReturnCreatedAtRoute_AndIndexInElasticSearch()
    {
        var mapper = new MapperConfiguration(conf => conf.AddProfile<PaperlessProfile>()).CreateMapper();
        // Arrange
        var mockService = new Mock<IDocumentService>();
        var mockFile = new Mock<IFormFile>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        var mockElasticSearchClient = new Mock<ElasticSearchClient>(); // Mock ElasticSearch client

        mockLoggerFactory
            .Setup(l => l.CreateLogger(It.IsAny<string>()))
            .Returns(mockLogger.Object);

        mockFile
            .Setup(l => l.ContentType)
            .Returns("application/pdf");

        mockService
            .Setup(r => r.CreateAsync(It.IsAny<Document>(), It.IsAny<IFile>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mockElasticSearchClient
            .Setup(es => es.IndexDocumentAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        var model = new UploadDocumentModel(mockFile.Object, "file.pdf", "Title", "Author");

        // Act
        var result = await DocumentEndpoints.UploadDocumentAsync(
            model,
            mockService.Object,
            mapper,
            mockLoggerFactory.Object,
            mockElasticSearchClient.Object
        );

        // Assert
        Assert.IsType<CreatedAtRoute>(result.Result);
        Assert.Equal("GetDocumentById", ((CreatedAtRoute)result.Result).RouteName);

        // Verify ElasticSearch was called
        mockElasticSearchClient.Verify(
            es => es.IndexDocumentAsync("documents", It.IsAny<object>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UploadDocumentAsync_ShouldReturnUnprocessableEntity_WhenModelIsInvalid()
    {
        var mapper = new MapperConfiguration(conf => conf.AddProfile<PaperlessProfile>()).CreateMapper();
        // Arrange
        var mockService = new Mock<IDocumentService>();
        var mockFile = new Mock<IFormFile>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        var mockElasticSearchClient = new Mock<ElasticSearchClient>(); // Mock ElasticSearch client

        mockLoggerFactory
            .Setup(l => l.CreateLogger(It.IsAny<string>()))
            .Returns(mockLogger.Object);

        mockFile
            .Setup(l => l.ContentType)
            .Returns("BADCONTENTTYPE/HERE");

        mockService
            .Setup(r => r.CreateAsync(It.IsAny<Document>(), It.IsAny<IFile>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var model = new UploadDocumentModel(mockFile.Object, "file.pdf");

        // Act
        var result = await DocumentEndpoints.UploadDocumentAsync(
            model,
            mockService.Object,
            mapper,
            mockLoggerFactory.Object,
            mockElasticSearchClient.Object
        );

        // Assert
        Assert.IsType<UnprocessableEntity>(result.Result);

        // Ensure ElasticSearchClient is NOT called
        mockElasticSearchClient.Verify(
            es => es.IndexDocumentAsync(It.IsAny<string>(), It.IsAny<object>()),
            Times.Never
        );

        // Ensure the logger recorded warnings
        mockLogger.Verify(
            l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()),
            Times.AtLeastOnce
        );
    }
}