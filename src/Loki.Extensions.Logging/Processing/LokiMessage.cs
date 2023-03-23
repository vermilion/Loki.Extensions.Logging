using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Loki.Extensions.Logging.Processing;

// https://grafana.com/docs/loki/latest/api/#push-log-entries-to-loki
public class LokiMessage
{
    public LokiMessage(string message, LogLevel logLevel)
    {
        Message = message;
        Level = GetLevel(logLevel);
        Timestamp = GetTimestamp();
    }

    public string? Message { get; set; }

    public decimal Timestamp { get; set; }

    public LokiSeverity Level { get; set; }

    public IReadOnlyCollection<KeyValuePair<string, object>> AdditionalFields { get; set; } =
        Array.Empty<KeyValuePair<string, object>>();

    private static decimal GetTimestamp()
    {
        var epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (DateTime.UtcNow - epochStart).Ticks * 100;
    }

    private static LokiSeverity GetLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => LokiSeverity.Debug,
            LogLevel.Debug => LokiSeverity.Debug,
            LogLevel.Information => LokiSeverity.Informational,
            LogLevel.Warning => LokiSeverity.Warning,
            LogLevel.Error => LokiSeverity.Error,
            LogLevel.Critical => LokiSeverity.Critical,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, "Log level not supported.")
        };
    }
}
