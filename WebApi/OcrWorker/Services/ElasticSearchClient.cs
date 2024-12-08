namespace OcrWorker.Services;

using Elastic.Clients.Elasticsearch;

public class ElasticSearchClient
{
    private readonly ElasticsearchClient _client;

    public ElasticSearchClient()
    {
        var uri = Environment.GetEnvironmentVariable("ELASTICSEARCH_URI");
        if (string.IsNullOrEmpty(uri))
        {
            throw new Exception("ElasticSearch URI not configured in environment variables.");
        }

        var settings = new ElasticsearchClientSettings(new Uri(uri));
        _client = new ElasticsearchClient(settings);
    }

    public async Task IndexDocumentAsync<T>(string indexName, T document) where T : class
    {
        var response = await _client.IndexAsync(document, idx => idx.Index(indexName));
        if (!response.IsValidResponse)
        {
            throw new Exception($"Failed to index document: {response.DebugInformation}");
        }
    }
}