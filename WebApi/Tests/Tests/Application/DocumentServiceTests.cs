using Application.Interfaces;
using Application.Services.Documents;
using Domain.Entities.Documents;
using Domain.Messaging;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Tests.Application;
public class DocumentServiceTests
{
    [Fact]
    public async Task CreateAsync_NullDocument_ThrowsArgumentNullException()
    {
        // Arrange
        var documentRepositoryMock = new Mock<IDocumentRepository>();
        var messageQueueServiceMock = new Mock<IMessageQueueService>();
        var fileStorageServiceMock = new Mock<IDocumentFileStorageService>();
        var loggerMock = new Mock<ILogger<DocumentService>>();

        var sut = new DocumentService(
            documentRepositoryMock.Object,
            messageQueueServiceMock.Object,
            fileStorageServiceMock.Object,
            loggerMock.Object);

        var fileMock = new Mock<IFile>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.CreateAsync(null!, fileMock.Object));
    }

    [Fact]
    public async Task CreateAsync_NullFile_ThrowsArgumentNullException()
    {
        // Arrange
        var documentRepositoryMock = new Mock<IDocumentRepository>();
        var messageQueueServiceMock = new Mock<IMessageQueueService>();
        var fileStorageServiceMock = new Mock<IDocumentFileStorageService>();
        var loggerMock = new Mock<ILogger<DocumentService>>();

        var sut = new DocumentService(
            documentRepositoryMock.Object,
            messageQueueServiceMock.Object,
            fileStorageServiceMock.Object,
            loggerMock.Object);

        var document = Document.New(DateTimeOffset.UtcNow, new DocumentMetadata("title", "author"));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.CreateAsync(document, null!));
    }

    [Fact]
    public async Task CreateAsync_SuccessfulUpload_CallsDependenciesInOrder()
    {
        // Arrange
        var documentRepositoryMock = new Mock<IDocumentRepository>();
        var messageQueueServiceMock = new Mock<IMessageQueueService>();
        var fileStorageServiceMock = new Mock<IDocumentFileStorageService>();
        var loggerMock = new Mock<ILogger<DocumentService>>();

        var sut = new DocumentService(
            documentRepositoryMock.Object,
            messageQueueServiceMock.Object,
            fileStorageServiceMock.Object,
            loggerMock.Object);

        var document = Document.New(DateTimeOffset.UtcNow, new DocumentMetadata("title", "author"));
        var fileMock = new Mock<IFile>();

        var ct = CancellationToken.None;

        // Setup sequence to verify order
        var sequence = new MockSequence();

        fileStorageServiceMock
            .InSequence(sequence)
            .Setup(s => s.UploadAsync(It.Is<DocumentFile>(df => df.Id == document.Id && df.File == fileMock.Object), ct))
            .Returns(Task.CompletedTask);

        documentRepositoryMock
            .InSequence(sequence)
            .Setup(r => r.CreateAsync(document, ct))
            .Returns(Task.CompletedTask);

        messageQueueServiceMock
            .InSequence(sequence)
            .Setup(m => m.Publish(It.Is<DocumentUploadedMessage>(msg => msg.DocumentId == document.Id)));

        // Act
        await sut.CreateAsync(document, fileMock.Object, ct);

        // Assert
        fileStorageServiceMock.Verify(s => s.UploadAsync(It.IsAny<DocumentFile>(), ct), Times.Once);
        documentRepositoryMock.Verify(r => r.CreateAsync(document, ct), Times.Once);
        messageQueueServiceMock.Verify(m => m.Publish(It.IsAny<DocumentUploadedMessage>()), Times.Once);

        // Verify the order of calls
        fileStorageServiceMock.Verify(s => s.UploadAsync(It.IsAny<DocumentFile>(), ct), Times.Once);
        documentRepositoryMock.Verify(r => r.CreateAsync(document, ct), Times.Once);
        messageQueueServiceMock.Verify(m => m.Publish(It.IsAny<DocumentUploadedMessage>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_FileUploadFails_RollsBackAndThrowsException()
    {
        // Arrange
        var documentRepositoryMock = new Mock<IDocumentRepository>();
        var messageQueueServiceMock = new Mock<IMessageQueueService>();
        var fileStorageServiceMock = new Mock<IDocumentFileStorageService>();
        var loggerMock = new Mock<ILogger<DocumentService>>();

        var sut = new DocumentService(
            documentRepositoryMock.Object,
            messageQueueServiceMock.Object,
            fileStorageServiceMock.Object,
            loggerMock.Object);

        var document = Document.New(DateTimeOffset.UtcNow, new DocumentMetadata("title", "author"));
        var fileMock = new Mock<IFile>();

        var ct = CancellationToken.None;

        fileStorageServiceMock
            .Setup(s => s.UploadAsync(It.IsAny<DocumentFile>(), ct))
            .ThrowsAsync(new Exception("File upload failed"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ApplicationException>(() => sut.CreateAsync(document, fileMock.Object, ct));

        // Verify rollback
        fileStorageServiceMock.Verify(s => s.DeleteAsync(document.Id, CancellationToken.None), Times.Once);
        documentRepositoryMock.Verify(r => r.DeleteAsync(document.Id, CancellationToken.None), Times.Once);

        // Verify that CreateAsync was not called
        documentRepositoryMock.Verify(r => r.CreateAsync(document, ct), Times.Never);

        // Verify that Publish was not called
        messageQueueServiceMock.Verify(m => m.Publish(It.IsAny<DocumentUploadedMessage>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DatabaseCreateFails_RollsBackAndThrowsException()
    {
        // Arrange
        var documentRepositoryMock = new Mock<IDocumentRepository>();
        var messageQueueServiceMock = new Mock<IMessageQueueService>();
        var fileStorageServiceMock = new Mock<IDocumentFileStorageService>();
        var loggerMock = new Mock<ILogger<DocumentService>>();

        var sut = new DocumentService(
            documentRepositoryMock.Object,
            messageQueueServiceMock.Object,
            fileStorageServiceMock.Object,
            loggerMock.Object);

        var document = Document.New(DateTimeOffset.UtcNow, new DocumentMetadata("title", "author"));
        var fileMock = new Mock<IFile>();

        var ct = CancellationToken.None;

        fileStorageServiceMock
            .Setup(s => s.UploadAsync(It.IsAny<DocumentFile>(), ct))
            .Returns(Task.CompletedTask);

        documentRepositoryMock
            .Setup(r => r.CreateAsync(document, ct))
            .ThrowsAsync(new Exception("Database create failed"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ApplicationException>(() => sut.CreateAsync(document, fileMock.Object, ct));

        // Verify rollback
        fileStorageServiceMock.Verify(s => s.DeleteAsync(document.Id, CancellationToken.None), Times.Once);
        documentRepositoryMock.Verify(r => r.DeleteAsync(document.Id, CancellationToken.None), Times.Once);

        // Verify that Publish was not called
        messageQueueServiceMock.Verify(m => m.Publish(It.IsAny<DocumentUploadedMessage>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_NullId_ThrowsArgumentNullException()
    {
        // Arrange
        var documentRepositoryMock = new Mock<IDocumentRepository>();
        var messageQueueServiceMock = new Mock<IMessageQueueService>();
        var fileStorageServiceMock = new Mock<IDocumentFileStorageService>();
        var loggerMock = new Mock<ILogger<DocumentService>>();

        var sut = new DocumentService(
            documentRepositoryMock.Object,
            messageQueueServiceMock.Object,
            fileStorageServiceMock.Object,
            loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.DeleteAsync(null!));
    }

    [Fact]
    public async Task DeleteAsync_ValidId_DeletesDocument()
    {
        // Arrange
        var documentRepositoryMock = new Mock<IDocumentRepository>();
        var messageQueueServiceMock = new Mock<IMessageQueueService>();
        var fileStorageServiceMock = new Mock<IDocumentFileStorageService>();
        var loggerMock = new Mock<ILogger<DocumentService>>();

        var sut = new DocumentService(
            documentRepositoryMock.Object,
            messageQueueServiceMock.Object,
            fileStorageServiceMock.Object,
            loggerMock.Object);

        var documentId = DocumentId.New();

        documentRepositoryMock
            .Setup(r => r.DeleteAsync(documentId, CancellationToken.None))
            .ReturnsAsync(true);

        // Act
        var result = await sut.DeleteAsync(documentId);

        // Assert
        Assert.True(result);
        documentRepositoryMock.Verify(r => r.DeleteAsync(documentId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetAsync_ReturnsDocuments()
    {
        // Arrange
        var documentRepositoryMock = new Mock<IDocumentRepository>();
        var messageQueueServiceMock = new Mock<IMessageQueueService>();
        var fileStorageServiceMock = new Mock<IDocumentFileStorageService>();
        var loggerMock = new Mock<ILogger<DocumentService>>();

        var sut = new DocumentService(
            documentRepositoryMock.Object,
            messageQueueServiceMock.Object,
            fileStorageServiceMock.Object,
            loggerMock.Object);

        var documents = new List<Document>
        {
            Document.New(DateTimeOffset.UtcNow, new DocumentMetadata("title1", "author1")),
            Document.New(DateTimeOffset.UtcNow, new DocumentMetadata("title2", "author2"))
        };

        documentRepositoryMock
            .Setup(r => r.GetAsync(CancellationToken.None))
            .ReturnsAsync(documents);

        // Act
        var result = await sut.GetAsync();

        // Assert
        Assert.Equal(documents, result);
        documentRepositoryMock.Verify(r => r.GetAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WithId_ReturnsDocument()
    {
        // Arrange
        var documentRepositoryMock = new Mock<IDocumentRepository>();
        var messageQueueServiceMock = new Mock<IMessageQueueService>();
        var fileStorageServiceMock = new Mock<IDocumentFileStorageService>();
        var loggerMock = new Mock<ILogger<DocumentService>>();

        var sut = new DocumentService(
            documentRepositoryMock.Object,
            messageQueueServiceMock.Object,
            fileStorageServiceMock.Object,
            loggerMock.Object);

        var document = Document.New(DateTimeOffset.UtcNow, new DocumentMetadata("title", "author"));

        documentRepositoryMock
            .Setup(r => r.GetAsync(document.Id, CancellationToken.None))
            .ReturnsAsync(document);

        // Act
        var result = await sut.GetAsync(document.Id);

        // Assert
        Assert.Equal(document, result);
        documentRepositoryMock.Verify(r => r.GetAsync(document.Id, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetAsync_NullId_ThrowsArgumentNullException()
    {
        // Arrange
        var documentRepositoryMock = new Mock<IDocumentRepository>();
        var messageQueueServiceMock = new Mock<IMessageQueueService>();
        var fileStorageServiceMock = new Mock<IDocumentFileStorageService>();
        var loggerMock = new Mock<ILogger<DocumentService>>();

        var sut = new DocumentService(
            documentRepositoryMock.Object,
            messageQueueServiceMock.Object,
            fileStorageServiceMock.Object,
            loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.GetAsync(null!));
    }
}
