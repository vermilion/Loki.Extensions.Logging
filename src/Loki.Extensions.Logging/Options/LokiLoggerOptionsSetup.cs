using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace Loki.Extensions.Logging.Options;

internal class LokiLoggerOptionsSetup : ConfigureFromConfigurationOptions<LokiLoggerOptions>
{
    public LokiLoggerOptionsSetup(ILoggerProviderConfiguration<LokiLoggerProvider> providerConfiguration)
        : base(providerConfiguration.Configuration)
    {
    }
}
