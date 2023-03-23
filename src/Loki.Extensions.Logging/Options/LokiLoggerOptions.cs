using Loki.Extensions.Logging.Processing;
using Microsoft.Extensions.Logging;

namespace Loki.Extensions.Logging.Options;

public class LokiLoggerOptions
{
    /// <summary>
    /// Enable/disable additional fields added via log scopes
    /// </summary>
    public bool IncludeScopes { get; set; } = true;

    /// <summary>
    /// Enable/disable additional predefined fields
    /// </summary>
    public bool IncludePredefinedFields { get; set; } = true;

    /// <summary>
    /// Loki server host
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// Loki server port
    /// </summary>
    public int Port { get; set; } = 3100;

    /// <summary>
    /// Log source name
    /// </summary>
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>
    /// Machine name
    /// </summary>
    public string? MachineName { get; set; } = Environment.MachineName;

    /// <summary>
    /// Additional fields that will be attached to all log messages
    /// </summary>
    public Dictionary<string, object> AdditionalFields { get; set; } = new();

    /// <summary>
    /// Additional fields computed based on raw log data.
    /// </summary>
    public Func<LogLevel, EventId, Exception?, Dictionary<string, object>>? AdditionalFieldsFactory { get; set; }

    /// <summary>
    /// Timeout used when sending logs via HTTP(S).
    /// </summary>
    public TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
