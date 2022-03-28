using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using WebSocketLibrary.Serialization;

namespace WebSocketLibrary
{
    /// <summary>
    /// Acts as a base class for websocket connection that encapsulates the methods needed for a functioning websocket
    /// communication.
    /// </summary>
    /// <typeparam name="TSend">The data that will be send to the connected websocket.</typeparam>
    /// <typeparam name="TReceive">The data received from the connected websocket.</typeparam>
    public abstract class WebSocketBase<TSend, TReceive>
    {
        private readonly ILogger logger;
        private readonly JsonSerializerOptions serializerOptions;
        private readonly Action<TReceive> receivedDataCallback;
        private readonly ActionBlock<TReceive> receivedDataCallbackActionBlock;
        private readonly BufferBlock<TSend> sendBufferBlock;
        protected CancellationTokenSource cancellationTokenSource;
        protected System.Net.WebSockets.WebSocket webSocket;

        public WebSocketBase(
            Action<TReceive> receivedDataCallback,
            ILogger logger,
            JsonSerializerOptions jsonSerializerOptions)
        {
            this.receivedDataCallback = receivedDataCallback ?? throw new ArgumentNullException(nameof(receivedDataCallback));
            this.logger = logger;
            this.serializerOptions = jsonSerializerOptions;
            this.cancellationTokenSource = new CancellationTokenSource();

            var dataFlowOptions = new ExecutionDataflowBlockOptions
            {
                EnsureOrdered = true,
                MaxDegreeOfParallelism = 1,
                CancellationToken = this.cancellationTokenSource.Token
            };

            this.receivedDataCallbackActionBlock = new ActionBlock<TReceive>(this.receivedDataCallback, dataFlowOptions);
            this.sendBufferBlock = new BufferBlock<TSend>(dataFlowOptions);
        }

        /// <summary>
        /// Gracefully closes the websocket connection.
        /// </summary>
        /// <returns>A task that completes when the operation finishes.</returns>
        public async Task CloseAsync()
        {
            if (this.webSocket.State != WebSocketState.Open) return;

            await this.webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, this.cancellationTokenSource.Token)
                .ConfigureAwait(false);
            this.webSocket?.Dispose();
        }

        /// <summary>
        /// Used to send data to the connected websocket.
        /// </summary>
        /// <param name="data">The data that will be sent to the connected websocket</param>
        public void Send(TSend data)
        {
            this.sendBufferBlock.Post(data);
        }

        /// <summary>
        /// Starts the reader and sender worker threads to make the websocket communication possible.
        /// </summary>
        public virtual async Task StartProcessingAsync()
        {
            try
            {
                var t1 = this.MessageReaderWorkerAsync(this.cancellationTokenSource.Token);
                var t2 = this.MessageSenderWorkerAsync(this.cancellationTokenSource.Token);
                var tx = await Task.WhenAny(t1, t2).ConfigureAwait(false);

                if (tx.Exception != null)
                {
                    throw tx.Exception;
                }
            }
            catch
            {
            }

            this.cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Continously reads from the websocket and once a full message has arrived, relays it.
        /// </summary>
        private async Task MessageReaderWorkerAsync(CancellationToken cancellationToken)
        {
            using (var bufferWriter = new ArrayPoolBufferWriter(4096))
            {
                while (cancellationToken.IsCancellationRequested == false)
                {
                    var buffer = bufferWriter.GetArraySegment(4096);
                    var result = await this.webSocket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
                    bufferWriter.Advance(result.Count);

                    if (result.Count == 0)
                    {
                        break;
                    }

                    if (result.EndOfMessage)
                    {
                        this.ProcessReceivedData(bufferWriter.OutputAsSpan);
                        bufferWriter.Reset();
                    }
                }
            }
        }

        /// <summary>
        /// Posts the data in the callback action block so it can be processed in FIFO order.
        /// </summary>
        private void ProcessReceivedData(ReadOnlySpan<byte> utf8Json)
        {
            try
            {
                var data = JsonSerializer.Deserialize<TReceive>(utf8Json, this.serializerOptions);

                if (data != null)
                {
                    this.receivedDataCallbackActionBlock.Post(data);
                }
            }
            catch 
            {
            }
        }

        /// <summary>
        /// Continously listens for data being posted into the bufferblock and relays it.
        /// </summary>
        private async Task MessageSenderWorkerAsync(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                var data = await this.sendBufferBlock.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                await this.SendDataAsync(data, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task SendDataAsync(TSend data, CancellationToken cancellationToken)
        {
            using (var poolWriter = new ArrayPoolBufferWriter(4096))
            using (var jsonWriter = new Utf8JsonWriter(poolWriter))
            {
                JsonSerializer.Serialize(jsonWriter, data, this.serializerOptions);
                await this.webSocket.SendAsync(poolWriter.OutputAsArraySegment, WebSocketMessageType.Text, true, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
