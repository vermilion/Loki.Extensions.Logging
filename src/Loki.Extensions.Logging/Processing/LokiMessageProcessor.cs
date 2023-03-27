using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using Loki.Extensions.Logging.Message;
using Loki.Extensions.Logging.Providers;

namespace Loki.Extensions.Logging.Processing;

public class LokiMessageProcessor : ILokiMessageProcessor
{
    private readonly BufferBlock<LokiMessage> _messageBuffer;
    private readonly Task _processorTask;
    private ILokiClient? _writer;

    public LokiMessageProcessor()
    {
        _messageBuffer = new BufferBlock<LokiMessage>(new DataflowBlockOptions
        {
            BoundedCapacity = 10000
        });

        _processorTask = Task.Run(StartAsync);
    }

    public void SetWriter(ILokiClient writer)
    {
        _writer = writer;
    }

    private async Task StartAsync()
    {
        while (!_messageBuffer.Completion.IsCompleted)
        {
            try
            {
                var message = await _messageBuffer.ReceiveAsync();

                if (_writer is not null)
                    await _writer.SendMessageAsync(message);
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

    public void EnqueueMessage(LokiMessage message)
    {
        if (!_messageBuffer.Post(message))
        {
            Debug.Fail("Failed to add Loki message to buffer.");
        }
    }

    public void Dispose()
    {
        _messageBuffer.Complete();
        _processorTask.Wait();

        _writer?.Dispose();
    }
}
