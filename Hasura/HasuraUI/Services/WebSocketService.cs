using SharedLibrary.Models;
using System;
using System.Text.Json;
using WebSocketLibrary;

namespace HasuraUI.Services
{
    public class WebSocketService
    {
        private WebSocketClient<SubscriptionRequest, SubscriptionResponse> webSocketClient;

        public WebSocketService(Action<SubscriptionResponse> callback)
        {
            this.webSocketClient = new WebSocketClient<SubscriptionRequest, SubscriptionResponse>(
                "wss://hasurawebsocketserver2022032823400.azurewebsites.net/ws",
                callback,
                null,
                new JsonSerializerOptions()
                );

            Console.WriteLine("Starting websockets");
            this.webSocketClient.StartProcessingAsync();
        }

        public void SendRequest(int id)
        {
            this.webSocketClient.Send(new SubscriptionRequest () { Id = id });
            Console.WriteLine("Sent id to websocket");
        }
    }
}
