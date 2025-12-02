using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
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
            Trace.Listeners.Add(new DefaultTraceListener());

            var uri = new Uri(_configuration["RabbitLogging:URI"] ?? "");
            Trace.WriteLine($"\"LoggerService\" => Elasticsearch uri : {uri.ToString()}");
            Trace.WriteLine($"\"LoggerService\" => Elasticsearch logging source : {_configuration["RabbitLogging:Source"]}");

            var settings = new ElasticsearchClientSettings(uri)
                .DefaultIndex(_indexName)
                .PrettyJson()
                .DisableDirectStreaming();

            _client = new ElasticsearchClient(settings);

            CreateIndexIfNotExists().Wait();
        }

        public Task LogInformation(string message, object? data = null)
            => LogAsync("INFO", message, null, data);

        public Task LogWarning(string message, object? data = null)
            => LogAsync("WARN", message, null, data);

        public Task LogError(string message, Exception ex, object? data = null)
            => LogAsync("ERROR", message, ex, data);

        #region Private Methods
        private async Task LogAsync(string level, string message, Exception? ex = null, object? data = null)
        {
            try
            {
                var log = new LogModel
                {
                    Level = level,
                    Message = message,
                    Timestamp = DateTime.UtcNow,
                    Exception = ex?.ToString(),
                    Source = _configuration["RabbitLogging:Source"] ?? throw new ArgumentException("Source can not be null or empty."),
                    AdditionalData = data != null ? ConvertToDict(data) : null
                };

                if(level == "ERROR" && ex != null)
                {
                    log.StackTrace = ex.StackTrace;
                }
                var response = await _client.IndexAsync(log);

                Trace.WriteLine($"\"LoggerService\" => Log level : {level} , Message : {message}, Logging status : {(response.IsSuccess() ? "Sucess" : "Failed")}");

                if (response != null && !response.IsValidResponse)
                {
                    Trace.WriteLine("Failed to index log: " + response.DebugInformation);
                    Trace.WriteLine("\"LoggerService\" => Failed to index log: " + response.DebugInformation);
                }
            }
            catch (Exception logEx)
            {
                Trace.WriteLine($"\"LoggerService\" => Failed to log to Elasticsearch: {logEx.Message}");
            }
        }

        private Dictionary<string, object> ConvertToDict(object obj) =>
            JsonSerializer.Deserialize<Dictionary<string, object>>(
                JsonSerializer.Serialize(obj)
            )!;

        private async Task CreateIndexIfNotExists()
        {
            var exists = await _client.Indices.ExistsAsync(_indexName);
            Trace.WriteLine($"\"LoggerService\" => Is index exist : {exists.Exists}. Index Name : {_indexName}");

            if (!exists.Exists)
            {
                await _client.Indices.CreateAsync(_indexName);
                Trace.WriteLine($"\"LoggerService\" => Created Index Name : {_indexName}");
            }
        }
        #endregion Private Methods
    }
}
