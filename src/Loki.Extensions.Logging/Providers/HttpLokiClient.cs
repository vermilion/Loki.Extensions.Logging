using System.Text.Json;
using Loki.Extensions.Logging.Options;
using Loki.Extensions.Logging.Processing;

namespace Loki.Extensions.Logging.Providers;

public class HttpLokiClient : ILokiClient
{
    private readonly HttpClient _httpClient;
    private const string PushEndpointV1 = "/loki/api/v1/push";

    public HttpLokiClient(LokiLoggerOptions options)
    {
        var uriBuilder = new UriBuilder
        {
            Scheme = "http",
            Host = options.Host,
            Port = options.Port
        };

        _httpClient = new HttpClient
        {
            BaseAddress = uriBuilder.Uri,
            Timeout = options.HttpTimeout
        };
    }

    public async Task SendMessageAsync(LokiMessage message)
    {
        var labels = message.AdditionalFields.ToDictionary(x => x.Key, x => x.Value?.ToString());

        var requestBody = new
        {
            streams = new[]
            {
                new
                {
                    stream = labels,
                    values = new[]
                    {
                        new[]
                        {
                            message.Timestamp.ToString(),
                            message.Message
                        }
                    }
                }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), null, "application/json");
        content.Headers.ContentType.CharSet = null; // Loki does not accept 'charset' in the Content-Type header

        var request = new HttpRequestMessage(HttpMethod.Post, PushEndpointV1)
        {
            Content = content
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
