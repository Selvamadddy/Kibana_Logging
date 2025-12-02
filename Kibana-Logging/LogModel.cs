
namespace Kibana_Logging
{
    public class LogModel
    {
        public required string Level { get; set; }
        public required string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Exception { get; set; }
        public string? StackTrace { get; set; }
        public string? Source { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }
}
