using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace WebSocketLibrary
{
    /// <summary>
    /// Acts as websocket server that encapsulates the methods needed for a functioning websocket communication. Inherits from
    /// <see cref="WebSocketBase{TSend, TReceive}" />.
    /// </summary>
    /// <typeparam name="TSend">The data that will be send to the connected websocket.</typeparam>
    /// <typeparam name="TReceive">The data received from the connected websocket.</typeparam>
    public class WebSocketServer<TSend, TReceive> : WebSocketBase<TSend, TReceive>
    {
        /// <summary>
        /// Takes a newly opened websocket connection.
        /// </summary>
        /// <param name="webSocket">An ongoing websocket connection.</param>
        /// <param name="receivedDataCallback">
        /// This method will be called when data from the websocket is received. Can be used for
        /// processing of the data.
        /// </param>
        /// <param name="logger">The logger.</param>
        /// <param name="jsonSerializerOptions">The serializer options needed for the particular implementation.</param>
        public WebSocketServer(
            System.Net.WebSockets.WebSocket webSocket,
            Action<TReceive> receivedDataCallback,
            ILogger logger,
            JsonSerializerOptions jsonSerializerOptions) : base(receivedDataCallback, logger, jsonSerializerOptions)
        {
            this.webSocket = webSocket;
        }
    }
}
