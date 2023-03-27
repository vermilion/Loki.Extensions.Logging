using System.Collections.Concurrent;
using Loki.Extensions.Logging.Options;
using Loki.Extensions.Logging.Processing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Loki.Extensions.Logging;

[ProviderAlias("Loki")]
public class LokiLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly ConcurrentDictionary<string, LokiLogger> _loggers = new();

    private readonly ILokiMessageProcessor _messageProcessor;
    private readonly IOptionsMonitor<LokiLoggerOptions> _optionsMonitor;
    private IExternalScopeProvider? _scopeProvider;

    public LokiLoggerProvider(ILokiMessageProcessor messageProcessor, IOptionsMonitor<LokiLoggerOptions> optionsMonitor)
    {
        _messageProcessor = messageProcessor;
        _optionsMonitor = optionsMonitor;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public ILogger CreateLogger(string name)
    {
        return _loggers.GetOrAdd(name, newName => new LokiLogger(newName, _messageProcessor, _optionsMonitor)
        {
            ScopeProvider = _scopeProvider
        });
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
        foreach (var logger in _loggers)
        {
            logger.Value.ScopeProvider = _scopeProvider;
        }
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Dispose()
    {
        _messageProcessor?.Dispose();
    }
}
