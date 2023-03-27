using Loki.Extensions.Logging.Message;
using Loki.Extensions.Logging.Providers;

namespace Loki.Extensions.Logging.Processing;

public interface ILokiMessageProcessor : IDisposable
{
    void SetWriter(ILokiClient writer);
    void EnqueueMessage(LokiMessage message);
}