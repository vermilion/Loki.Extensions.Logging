using System;
using System.Threading.Tasks;
using Loki.Extensions.Logging.Processing;

namespace Loki.Extensions.Logging.Providers;

public interface ILokiClient : IDisposable
{
    Task SendMessageAsync(LokiMessage message);
}
