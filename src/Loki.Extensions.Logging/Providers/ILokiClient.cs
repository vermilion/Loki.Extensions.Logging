using Loki.Extensions.Logging.Message;

namespace Loki.Extensions.Logging.Providers;

public interface ILokiClient : IDisposable
{
    Task SendMessageAsync(LokiMessage message);
}
