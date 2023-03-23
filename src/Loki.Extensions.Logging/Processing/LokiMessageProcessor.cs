using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Loki.Extensions.Logging.Providers;

namespace Loki.Extensions.Logging.Processing;

public class LokiMessageProcessor
{
    private readonly BufferBlock<LokiMessage> _messageBuffer;

    private Task _processorTask = Task.CompletedTask;

    public LokiMessageProcessor(ILokiClient lokiClient)
    {
        LokiClient = lokiClient;

        _messageBuffer = new BufferBlock<LokiMessage>(new DataflowBlockOptions
        {
            BoundedCapacity = 10000
        });
    }

    internal ILokiClient LokiClient { get; set; }

    public void Start()
    {
        _processorTask = Task.Run(StartAsync);
    }

    private async Task StartAsync()
    {
        while (!_messageBuffer.Completion.IsCompleted)
        {
            try
            {
                var message = await _messageBuffer.ReceiveAsync();
                await LokiClient.SendMessageAsync(message);
            }
            catch (InvalidOperationException)
            {
                // The source completed without providing data to receive.
            }
            catch (Exception ex)
            {
                Debug.Fail("Unhandled exception while sending Loki message.", ex.ToString());
            }
        }
    }

    public void Stop()
    {
        _messageBuffer.Complete();
        _processorTask.Wait();
    }

    public void SendMessage(LokiMessage message)
    {
        if (!_messageBuffer.Post(message))
        {
            Debug.Fail("Failed to add Loki message to buffer.");
        }
    }
}
