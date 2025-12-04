using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text.Json;

namespace Kibana_Logging
{
    public class LoggerService : ILoggerService
    {
        private readonly ElasticsearchClient _client;
        private readonly string _indexName = "dev-rabbit";
        private IConfiguration _configuration;
        private readonly LogLevel _logLevel;

        public LoggerService(IConfiguration configuration)
        {
            _configuration = configuration;
            Trace.Listeners.Add(new DefaultTraceListener());

            var uri = new Uri(_configuration["RabbitLogging:URI"] ?? "");
            Trace.WriteLine($"\"LoggerService\" => Elasticsearch uri : {uri?.ToString()}");
            Trace.WriteLine($"\"LoggerService\" => Elasticsearch logging source : {_configuration["RabbitLogging:Source"]}");

            var settings = new ElasticsearchClientSettings(uri)
                .DefaultIndex(_indexName)
                .PrettyJson()
                .DisableDirectStreaming();

            _client = new ElasticsearchClient(settings);

            CreateIndexIfNotExists().Wait();

            _logLevel = GetLogLevel();
        }

        public Task LogInformation(string message, object? data = null)
            => LogAsync(LogLevel.Information, message, null, data);

        public Task LogDebug(string message, object? data = null)
            => LogAsync(LogLevel.Debug, message, null, data);

        public Task LogWarning(string message, object? data = null)
            => LogAsync(LogLevel.Warning, message, null, data);

        public Task LogError(string message, Exception ex, object? data = null)
            => LogAsync(LogLevel.Error, message, ex, data);

        public Task LogCritical(string message, Exception ex, object? data = null)
            => LogAsync(LogLevel.Critical, message, ex, data);

        #region Private Methods
        private async Task LogAsync(LogLevel logLevel, string message, Exception? ex = null, object? data = null)
        {
            try
            {
                if (logLevel < _logLevel)
                    return;

                var log = new LogModel
                {
                    Level = logLevel.ToString(),
                    Message = message,
                    Timestamp = DateTime.UtcNow,
                    Exception = ex?.ToString(),
                    Source = _configuration["RabbitLogging:Source"] ?? throw new ArgumentException("Source can not be null or empty."),
                    AdditionalData = data != null ? ConvertToDict(data) : null
                };

                if((logLevel == LogLevel.Error || logLevel == LogLevel.Critical)  && ex != null)
                {
                    log.StackTrace = ex.StackTrace;
                }
                var response = await _client.IndexAsync(log);

                Trace.WriteLine($"\"LoggerService\" => Log level : {logLevel} , Message : {message}, Logging status : {(response.IsSuccess() ? "Sucess" : "Failed")}");

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

        private LogLevel GetLogLevel()
        {
            var configuredLogLevel = _configuration["RabbitLogging:LogLevel"] ?? "Information";
            Trace.WriteLine($"\"LoggerService\" => Configured Log Level : {configuredLogLevel}");
            return Enum.TryParse<LogLevel>(configuredLogLevel, true, out var parsed) ? parsed : LogLevel.Information;
        }
        #endregion Private Methods
    }
}
