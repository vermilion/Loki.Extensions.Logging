using Loki.Extensions.Logging.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace Loki.Extensions.Logging;

public static class LoggingBuilderExtensions
{
    /// <summary>
    ///     Registers a <see cref="LokiLoggerProvider" /> with the service collection.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static ILoggingBuilder AddLoki(this ILoggingBuilder builder)
    {
        builder.AddConfiguration();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LokiLoggerProvider>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<LokiLoggerOptions>, LokiLoggerOptionsSetup>());
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IOptionsChangeTokenSource<LokiLoggerOptions>,
                LoggerProviderOptionsChangeTokenSource<LokiLoggerOptions, LokiLoggerProvider>>());

        return builder;
    }

    /// <summary>
    ///     Registers a <see cref="LokiLoggerProvider" /> with the service collection allowing logger options
    ///     to be customised.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static ILoggingBuilder AddLoki(this ILoggingBuilder builder, Action<LokiLoggerOptions> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        builder.AddLoki();
        builder.Services.Configure(configure);
        return builder;
    }
}
