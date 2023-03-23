using System;
using System.Collections.Concurrent;
using Loki.Extensions.Logging.Helpers;
using Loki.Extensions.Logging.Logger;
using Loki.Extensions.Logging.Options;
using Loki.Extensions.Logging.Processing;
using Loki.Extensions.Logging.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Loki.Extensions.Logging;

[ProviderAlias("Loki")]
public class LokiLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly IOptionsMonitor<LokiLoggerOptions> _options;
    private readonly ConcurrentDictionary<string, LokiLogger> _loggers;
    private readonly IDisposable _optionsReloadToken;

    private ILokiClient? _lokiClient;
    private LokiMessageProcessor? _messageProcessor;
    private IExternalScopeProvider? _scopeProvider;

    public LokiLoggerProvider(IOptionsMonitor<LokiLoggerOptions> options)
    {
        _options = options;
        _loggers = new ConcurrentDictionary<string, LokiLogger>();

        LoadLoggerOptions(options.CurrentValue);

        var onOptionsChanged = Debouncer.Debounce<LokiLoggerOptions>(LoadLoggerOptions, TimeSpan.FromSeconds(1));
        _optionsReloadToken = options.OnChange(onOptionsChanged);
    }

    public ILogger CreateLogger(string name)
    {
        return _loggers.GetOrAdd(name, newName => new LokiLogger(newName, _messageProcessor!, _options.CurrentValue)
        {
            ScopeProvider = _scopeProvider
        });
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
        foreach (var logger in _loggers)
        {
            logger.Value.ScopeProvider = _scopeProvider;
        }
    }

    private void LoadLoggerOptions(LokiLoggerOptions options)
    {
        if (string.IsNullOrEmpty(options.Host))
        {
            throw new ArgumentException("Loki host is required.", nameof(options));
        }

        if (string.IsNullOrEmpty(options.ApplicationName))
        {
            throw new ArgumentException("Loki application name is required.", nameof(options));
        }

        var client = new HttpLokiClient(options);

        if (_messageProcessor == null)
        {
            _messageProcessor = new LokiMessageProcessor(client);
            _messageProcessor.Start();
        }
        else
        {
            _messageProcessor.LokiClient = client;
            _lokiClient?.Dispose();
        }

        _lokiClient = client;

        foreach (var logger in _loggers)
        {
            logger.Value.Options = options;
        }
    }

    public void Dispose()
    {
        _messageProcessor?.Stop();
        _lokiClient?.Dispose();
        _optionsReloadToken.Dispose();
    }
}
