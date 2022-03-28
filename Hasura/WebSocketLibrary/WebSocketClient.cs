using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebSocketLibrary
{
    /// <summary>
    /// Acts as websocket client that encapsulates the methods needed for a functioning websocket communication. Inherits from
    /// <see cref="WebSocketBase{TSend, TReceive}" />.
    /// </summary>
    /// <typeparam name="TSend">The data that will be send to the connected websocket.</typeparam>
    /// <typeparam name="TReceive">The data received from the connected websocket.</typeparam>
    public class WebSocketClient<TSend, TReceive> : WebSocketBase<TSend, TReceive>
    {
        private readonly string connectionString;

        /// <summary>
        /// Takes a connection string for a websocket connection.
        /// </summary>
        /// <param name="connectionString">The connection string for the websocket connection.</param>
        /// <param name="receivedDataCallback">
        /// This method will be called when data from the websocket is received. Can be used for
        /// processing of the data.
        /// </param>
        /// <param name="logger">The logger.</param>
        /// <param name="jsonSerializerOptions">The serializer options needed for the particular implementation.</param>
        public WebSocketClient(
            string connectionString,
            Action<TReceive> receivedDataCallback,
            ILogger logger,
            JsonSerializerOptions jsonSerializerOptions) : base(receivedDataCallback, logger, jsonSerializerOptions)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Starts the websocket connection. <inheritdoc />
        /// </summary>
        /// <returns></returns>
        public override async Task StartProcessingAsync()
        {
            await this.ConnectAsync().ConfigureAwait(false);
            await base.StartProcessingAsync().ConfigureAwait(false);
        }

        private async Task ConnectAsync()
        {
            if (this.webSocket != null ||
                this.webSocket?.State == WebSocketState.Open ||
                this.webSocket?.State == WebSocketState.Connecting) return;

            var clientWebSocket = new ClientWebSocket();
            await clientWebSocket.ConnectAsync(new Uri(this.connectionString), this.cancellationTokenSource.Token).ConfigureAwait(false);
            this.webSocket = clientWebSocket;
        }
    }
}
