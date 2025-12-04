namespace Kibana_Logging
{
    /// <summary>
    /// Provides logging functionality for writing structured logs to Elasticsearch or other outputs.
    /// </summary>
    public interface ILoggerService
    {
        /// <summary>
        /// Writes an informational log message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="data">Optional additional data to include in the log.</param>
        Task LogInformation(string message, object? data = null);

        /// <summary>
        /// Writes a warning log message.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        /// <param name="data">Optional additional data to include in the log.</param>
        Task LogWarning(string message, object? data = null);

        /// <summary>
        /// Writes an error log message including exception details and stack trace.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        /// <param name="ex">The exception that was thrown.</param>
        /// <param name="data">Optional additional contextual data to include in the log.</param>
        Task LogError(string message, Exception ex, object? data = null);

        /// <summary>
        /// Writes an debug log message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="data">Optional additional data to include in the log.</param>
        Task LogDebug(string message, object? data = null);

        /// <summary>
        /// Writes an cirtical log message including exception details and stack trace.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        /// <param name="ex">The exception that was thrown.</param>
        /// <param name="data">Optional additional contextual data to include in the log.</param>
        Task LogCritical(string message, Exception ex, object? data = null);
    }
}
