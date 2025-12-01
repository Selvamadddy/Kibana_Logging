using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Kibana_Logging
{
    public class LoggerService : ILoggerService
    {
        private IConfiguration _configuration;
        private readonly ElasticsearchClient _client;
        private string _indexName;
        public LoggerService(IConfiguration configuration)
        {
            _configuration = configuration;

            var node = new Uri("http://localhost:9200");

            var settings = new ElasticsearchClientSettings(node)
                                    .DefaultIndex("Dev-rabbit")
                                    .PrettyJson();
            // 👈 Forces compatible-with=8


            //if (!string.IsNullOrEmpty(username))
            //{
            //    settings.Authentication(new BasicAuthentication(username, password!));
            //}

            _client = new ElasticsearchClient(settings);
            _indexName = "Dev-rabbit";
        }

        public Task LogInformation(string message, object? data = null)
            => LogAsync("INFO", message, null, data);

        public Task LogWarning(string message, object? data = null)
            => LogAsync("WARN", message, null, data);

        public Task LogError(string message, Exception ex, object? data = null)
            => LogAsync("ERROR", message, ex, data);

        #region Private Methods
        private async Task LogAsync(string level, string message, Exception? exception = null, object? data = null)
        {
            var log = new LogModel
            {
                Level = level,
                Message = message,
                Exception = exception?.ToString(),
                Source = AppDomain.CurrentDomain.FriendlyName,
                AdditionalData = data != null ? ConvertObjectToDictionary(data) : null,
            };

            await _client.IndexAsync(log, idx => idx.Index(_indexName));

            var response = await _client.GetAsync<LogModel>(1, x => x.Index(_indexName));

            int aaaaaaaaaaaaa = 2;
        }
        private Dictionary<string, object> ConvertObjectToDictionary(object obj) =>
            JsonSerializer.Deserialize<Dictionary<string, object>>(
                JsonSerializer.Serialize(obj)
            )!;
        #endregion Private Methods
    }
}
