using Domain.Entities.Documents;
using Domain.Services.Documents;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.ElasticSearch;

file record DocumentIndex(DocumentId Id, string Content);

public class ElasticSearchDocumentIndexedService(IElasticsearchClientSettings settings, ILogger<ElasticSearchDocumentIndexedService> logger) : IDocumentIndexService
{
    private const string IndexName = "documents";
    private readonly ElasticsearchClient _client = new ElasticsearchClient(settings);

    public async Task<bool> StoreAsync(DocumentId id, string content, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(content);

        try
        {
            var document = new DocumentIndex(id, content);

            // Index the document into Elasticsearch
            var response = await _client.IndexAsync(
                document,
                request => request
                    .Index(IndexName)
                    .Id(id.ToString()),
                ct);

            // Check if the operation was successful
            return response.IsValidResponse;
        } catch (Exception ex)
        {
            logger.LogError(ex, "Failed to index document \"{documentId}\"", id);
            throw;
        }
    }

    public Task<bool> DeleteAsync(DocumentId id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetAsync(DocumentId id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<DocumentId>> SearchAsync(string query, CancellationToken ct = default)
    {
        var response = await _client.SearchAsync<DocumentIndex>(
            request => request
                .Index(IndexName)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.Content)
                        .Query(query)
                    )
                ),
            ct);

        return response.Documents.Select(d => d.Id).ToList();
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}
