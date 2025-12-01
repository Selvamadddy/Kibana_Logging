using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Kibana_Logging
{
    public class LoggerService : ILoggerService
    {
        private readonly ElasticsearchClient _client;
        private string _indexName = "dev-rabbit";
        private IConfiguration _configuration;

        public LoggerService(IConfiguration configuration)
        {
            _configuration = configuration;

            var uri = new Uri(_configuration["RabbitLogging:URI"] ?? "");
            var settings = new ElasticsearchClientSettings(uri)
                .DefaultIndex(_indexName)
                .PrettyJson()
                .DisableDirectStreaming();  // Helps debugging

            _client = new ElasticsearchClient(settings);

            CreateIndexIfNotExists().Wait();
        }

        private async Task CreateIndexIfNotExists()
        {
            var exists = await _client.Indices.ExistsAsync(_indexName);

            if (!exists.Exists)
            {
                await _client.Indices.CreateAsync(_indexName);
            }
        }


        public Task LogInformation(string message, object? data = null)
            => LogAsync("INFO", message, null, data);

        public Task LogWarning(string message, object? data = null)
            => LogAsync("WARN", message, null, data);

        public Task LogError(string message, Exception ex, object? data = null)
            => LogAsync("ERROR", message, ex, data);

        private async Task LogAsync(string level, string message, Exception? ex = null, object? data = null)
        {
            var log = new LogModel
            {
                Level = level,
                Message = message,
                Timestamp = DateTime.UtcNow,
                Exception = ex?.ToString(),
                Source = _configuration["RabbitLogging:Source"] ?? "gopal",
                AdditionalData = data != null ? ConvertToDict(data) : null
            };

            var response = await _client.IndexAsync(log);

            if (!response.IsValidResponse)
            {
                Console.WriteLine("Failed to index log: " + response.DebugInformation);
            }
        }

        private Dictionary<string, object> ConvertToDict(object obj) =>
            JsonSerializer.Deserialize<Dictionary<string, object>>(
                JsonSerializer.Serialize(obj)
            )!;
    }
}
