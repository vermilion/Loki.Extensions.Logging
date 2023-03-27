using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Loki.Extensions.Logging.Message;
using Loki.Extensions.Logging.Options;
using Loki.Extensions.Logging.Processing;
using Loki.Extensions.Logging.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Loki.Extensions.Logging;

/// <summary>
/// Grafana Loki logger instance
/// </summary>
public class LokiLogger : ILogger, IDisposable
{
    private static readonly Regex AdditionalFieldKeyRegex = new(@"^[\w\.\-]*$", RegexOptions.Compiled);

    private readonly string _name;
    private readonly ILokiMessageProcessor _messageProcessor;
    private readonly IDisposable? _optionsChangeToken;
    private LokiLoggerOptions _options;

    /// <summary>
    /// Constructor
    /// </summary>
    public LokiLogger(string name, ILokiMessageProcessor messageProcessor, IOptionsMonitor<LokiLoggerOptions> options)
    {
        _name = name;
        _messageProcessor = messageProcessor;

        _optionsChangeToken = options.OnChange(UpdateOptions);
        UpdateOptions(options.CurrentValue);
    }

    internal IExternalScopeProvider? ScopeProvider { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var messageText = formatter(state, exception);
        if (exception is not null)
            messageText += Environment.NewLine + exception.ToString();

        var message = new LokiMessage(messageText, logLevel)
        {
            Message = messageText,
            AdditionalFields = GetAdditionalFields(logLevel, eventId, exception).ToArray()
        };

        _messageProcessor.EnqueueMessage(message);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return ScopeProvider?.Push(state);
    }

    private void UpdateOptions(LokiLoggerOptions options)
    {
        _options = options;

        if (string.IsNullOrEmpty(options.Host))
        {
            Debug.Fail("Loki host is required");
            return;
        }

        if (string.IsNullOrEmpty(options.ApplicationName))
        {
            Debug.Fail("Loki application name is required");
            return;
        }

        _messageProcessor.SetWriter(new HttpLokiClient(options));
    }

    private IEnumerable<KeyValuePair<string, object>> GetAdditionalFields(LogLevel logLevel, EventId eventId, Exception? exception)
    {
        var additionalFields = _options.AdditionalFields
            .Concat(GetLokiAdditionalFields(logLevel))
            .Concat(GetFactoryAdditionalFields(logLevel, eventId, exception))
            .Concat(GetScopeAdditionalFields())
            .Concat(GetPredefinedAdditionalFields(exception));

        foreach (var field in additionalFields)
        {
            if (field.Key != "{OriginalFormat}")
            {
                if (!AdditionalFieldKeyRegex.IsMatch(field.Key))
                {
                    Debug.Fail($"Loki message has additional field with invalid key \"{field.Key}\".");
                    continue;
                }

                yield return field;
            }
        }
    }

    private IEnumerable<KeyValuePair<string, object>> GetLokiAdditionalFields(LogLevel logLevel)
    {
        var additionalFields = new Dictionary<string, object>
        {
            ["Level"] = logLevel.ToString()
        };

        return additionalFields.ToArray();
    }

    private IEnumerable<KeyValuePair<string, object>> GetFactoryAdditionalFields(LogLevel logLevel, EventId eventId, Exception? exception)
    {
        return _options.AdditionalFieldsFactory?.Invoke(logLevel, eventId, exception) ??
               Enumerable.Empty<KeyValuePair<string, object>>();
    }

    private IEnumerable<KeyValuePair<string, object>> GetScopeAdditionalFields()
    {
        if (!_options.IncludeScopes)
        {
            return Enumerable.Empty<KeyValuePair<string, object>>();
        }

        var additionalFields = new List<KeyValuePair<string, object>>();

        ScopeProvider?.ForEachScope((scope, state) =>
        {
            switch (scope)
            {
                case IEnumerable<KeyValuePair<string, object>> fields:
                    state.AddRange(fields);
                    break;
                case ValueTuple<string, string>(var key, var value):
                    state.Add(new KeyValuePair<string, object>(key, value));
                    break;
                case ValueTuple<string, int>(var key, var value):
                    state.Add(new KeyValuePair<string, object>(key, value));
                    break;
                case ValueTuple<string, long>(var key, var value):
                    state.Add(new KeyValuePair<string, object>(key, value));
                    break;
                case ValueTuple<string, short>(var key, var value):
                    state.Add(new KeyValuePair<string, object>(key, value));
                    break;
                case ValueTuple<string, decimal>(var key, var value):
                    state.Add(new KeyValuePair<string, object>(key, value));
                    break;
                case ValueTuple<string, double>(var key, var value):
                    state.Add(new KeyValuePair<string, object>(key, value));
                    break;
                case ValueTuple<string, float>(var key, var value):
                    state.Add(new KeyValuePair<string, object>(key, value));
                    break;
                case ValueTuple<string, uint>(var key, var value):
                    state.Add(new KeyValuePair<string, object>(key, value));
                    break;
                case ValueTuple<string, ulong>(var key, var value):
                    state.Add(new KeyValuePair<string, object>(key, value));
                    break;
                case ValueTuple<string, ushort>(var key, var value):
                    state.Add(new KeyValuePair<string, object>(key, value));
                    break;
                case ValueTuple<string, byte>(var key, var value):
                    state.Add(new KeyValuePair<string, object>(key, value));
                    break;
                case ValueTuple<string, sbyte>(var key, var value):
                    state.Add(new KeyValuePair<string, object>(key, value));
                    break;
                case ValueTuple<string, object>(var key, var value):
                    state.Add(new KeyValuePair<string, object>(key, value));
                    break;
            }
        }, additionalFields);

        return additionalFields;
    }

    private IEnumerable<KeyValuePair<string, object>> GetPredefinedAdditionalFields(Exception? exception)
    {
        if (!_options.IncludePredefinedFields)
        {
            return Enumerable.Empty<KeyValuePair<string, object>>();
        }

        var additionalFields = new Dictionary<string, object>();

        if (exception is not null)
            additionalFields["ExceptionType"] = exception.GetType().ToString();

        additionalFields["AppName"] = _options.ApplicationName;

        additionalFields["AppVersion"] = Assembly.GetEntryAssembly()
             !.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
             !.InformationalVersion;

        if (_options.MachineName is not null)
            additionalFields["MachineName"] = _options.MachineName;

        additionalFields["Category"] = _name;

        return additionalFields.ToArray();
    }

    public void Dispose()
    {
        _optionsChangeToken?.Dispose();
    }
}
