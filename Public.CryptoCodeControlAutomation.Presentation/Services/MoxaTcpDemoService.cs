using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;

namespace CryptoCodeControlAutomation.Presentation.Services
{
    public sealed class MoxaTcpDemoService : IAsyncDisposable
    {
        private static readonly TimeSpan MessageIdleTimeout = TimeSpan.FromMilliseconds(200);
        private readonly object _stateLock = new();
        private readonly ConcurrentDictionary<Guid, Channel<MoxaTcpMessage>> _subscribers = new();

        private TcpListener? _listener;
        private CancellationTokenSource? _listenerCancellationTokenSource;
        private Task? _listenerTask;
        private int? _port;

        public bool IsRunning
        {
            get
            {
                lock (_stateLock)
                {
                    return _listener != null;
                }
            }
        }

        public int? Port
        {
            get
            {
                lock (_stateLock)
                {
                    return _port;
                }
            }
        }

        public Task StartAsync(int port)
        {
            lock (_stateLock)
            {
                if (_listener != null)
                {
                    if (_port == port)
                        return Task.CompletedTask;

                    throw new InvalidOperationException($"{_port} portu zaten dinleniyor.");
                }

                var listener = new TcpListener(IPAddress.Any, port);
                listener.Start();

                var cancellationTokenSource = new CancellationTokenSource();
                _listener = listener;
                _listenerCancellationTokenSource = cancellationTokenSource;
                _listenerTask = ListenAsync(listener, cancellationTokenSource.Token);
                _port = port;
            }

            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            TcpListener? listener;
            CancellationTokenSource? cancellationTokenSource;
            Task? listenerTask;

            lock (_stateLock)
            {
                listener = _listener;
                cancellationTokenSource = _listenerCancellationTokenSource;
                listenerTask = _listenerTask;

                _listener = null;
                _listenerCancellationTokenSource = null;
                _listenerTask = null;
                _port = null;
            }

            if (listener == null)
                return;

            cancellationTokenSource?.Cancel();
            listener.Stop();

            if (listenerTask != null)
            {
                try
                {
                    await listenerTask;
                }
                catch (OperationCanceledException)
                {
                }
                catch (SocketException)
                {
                }
            }

            cancellationTokenSource?.Dispose();
        }

        public (Guid SubscriptionId, ChannelReader<MoxaTcpMessage> Reader) Subscribe()
        {
            var channel = Channel.CreateBounded<MoxaTcpMessage>(new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            });

            var subscriptionId = Guid.NewGuid();
            _subscribers[subscriptionId] = channel;
            return (subscriptionId, channel.Reader);
        }

        public void Unsubscribe(Guid subscriptionId)
        {
            if (_subscribers.TryRemove(subscriptionId, out var channel))
                channel.Writer.TryComplete();
        }

        public void PublishTestMessage(string value)
        {
            Publish(value, "Test2");
        }

        private async Task ListenAsync(TcpListener listener, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client;

                try
                {
                    client = await listener.AcceptTcpClientAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (SocketException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                _ = HandleClientAsync(client, cancellationToken);
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            using (client)
            {
                var source = client.Client.RemoteEndPoint?.ToString() ?? "Bilinmeyen istemci";
                var stream = client.GetStream();
                var buffer = new byte[4096];
                var pending = new StringBuilder();

                while (!cancellationToken.IsCancellationRequested)
                {
                    using var readCancellationTokenSource =
                        CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                    if (pending.Length > 0)
                        readCancellationTokenSource.CancelAfter(MessageIdleTimeout);

                    int readCount;

                    try
                    {
                        readCount = await stream.ReadAsync(buffer.AsMemory(), readCancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested && pending.Length > 0)
                    {
                        //throw;
                        PublishPendingMessage(pending, source);
                        continue;
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (IOException)
                    {
                        break;
                    }
                    catch (SocketException)
                    {
                        break;
                    }

                    if (readCount == 0)
                    {
                        PublishPendingMessage(pending, source);
                        break;
                    }

                    pending.Append(Encoding.UTF8.GetString(buffer, 0, readCount));
                    PublishCompleteMessages(pending, source);
                }
            }
        }

        private void PublishCompleteMessages(StringBuilder pending, string source)
        {
            while (true)
            {
                var separatorIndex = FindSeparatorIndex(pending);
                if (separatorIndex < 0)
                    return;

                var value = pending.ToString(0, separatorIndex);
                var removeLength = separatorIndex + 1;

                while (removeLength < pending.Length &&
                       (pending[removeLength] == '\r' || pending[removeLength] == '\n'))
                {
                    removeLength++;
                }

                pending.Remove(0, removeLength);
                Publish(value, source);
            }
        }

        private static int FindSeparatorIndex(StringBuilder value)
        {
            for (var index = 0; index < value.Length; index++)
            {
                if (value[index] is '\r' or '\n')
                    return index;
            }

            return -1;
        }

        private void PublishPendingMessage(StringBuilder pending, string source)
        {
            if (pending.Length == 0)
                return;

            var value = pending.ToString();
            pending.Clear();
            Publish(value, source);
        }

        private void Publish(string value, string source)
        {
            var normalizedValue = value.Trim('\0', '\r', '\n');
            if (string.IsNullOrWhiteSpace(normalizedValue))
                return;

            var message = new MoxaTcpMessage(normalizedValue, source, DateTimeOffset.Now);

            foreach (var subscriber in _subscribers.Values)
                subscriber.Writer.TryWrite(message);
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();

            foreach (var subscriptionId in _subscribers.Keys)
                Unsubscribe(subscriptionId);
        }
    }

    public sealed record MoxaTcpMessage(string Value, string Source, DateTimeOffset ReceivedAt);
}
