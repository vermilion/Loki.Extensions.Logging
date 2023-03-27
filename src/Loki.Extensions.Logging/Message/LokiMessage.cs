using Microsoft.Extensions.Logging;

namespace Loki.Extensions.Logging.Message;

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
            LogLevel.Trace => LokiSeverity.TRCE,
            LogLevel.Debug => LokiSeverity.DBUG,
            LogLevel.Information => LokiSeverity.INFO,
            LogLevel.Warning => LokiSeverity.WARN,
            LogLevel.Error => LokiSeverity.EROR,
            LogLevel.Critical => LokiSeverity.CRIT,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, "Log level not supported.")
        };
    }
}
