using System;
using Loki.Extensions.Logging.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Loki.Extensions.Logging;

public static class LoggerFactoryExtensions
{
    /// <summary>
    /// Adds a <see cref="LokiLoggerProvider" /> to the logger factory with the supplied <see cref="LokiLoggerOptions" />
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ILoggerFactory AddLoki(this ILoggerFactory loggerFactory, IOptionsMonitor<LokiLoggerOptions> options)
    {
        loggerFactory.AddProvider(new LokiLoggerProvider(options));
        return loggerFactory;
    }

    /// <summary>
    ///     Adds a <see cref="LokiLoggerProvider" /> to the logger factory with the supplied
    ///     <see cref="LokiLoggerOptions" />.
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static ILoggerFactory AddLoki(this ILoggerFactory loggerFactory, LokiLoggerOptions options)
    {
        return loggerFactory.AddLoki(new OptionsMonitorStub<LokiLoggerOptions>(options));
    }

    private class OptionsMonitorStub<T> : IOptionsMonitor<T>
    {
        public OptionsMonitorStub(T options)
        {
            CurrentValue = options;
        }

        public T CurrentValue { get; }

        public T Get(string name) => CurrentValue;

        public IDisposable OnChange(Action<T, string> listener) => new NullDisposable();

        private class NullDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
