using SharedLibrary.Models;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;
using WebSocketLibrary;

public class WebSocketSessionHandler
{
    private readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions();
    private readonly System.Net.WebSockets.WebSocket webSocket;
    private readonly ILoggerFactory loggerFactory;
    private WebSocketServer<SubscriptionResponse, SubscriptionRequest> webSocketServer;
    private SubscriptionService subscriptionService;

    public WebSocketSessionHandler(
        System.Net.WebSockets.WebSocket webSocket,
        SubscriptionService subscriptionService,
        ILoggerFactory loggerFactory)
    {
        this.webSocket = webSocket;
        this.subscriptionService = subscriptionService;
        this.loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Processes the websocket by continuously receiving messages.
    /// </summary>
    public async Task ProcessAsync()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            this.webSocketServer = new WebSocketServer<SubscriptionResponse, SubscriptionRequest>(
                this.webSocket,
                subscriptionService.RelayData,
                this.loggerFactory.CreateLogger<WebSocketServer<SubscriptionResponse, SubscriptionRequest>>(),
                this.serializerOptions);

            _ = this.webSocketServer.StartProcessingAsync();
            await this.ProcessResponseData(cancellationTokenSource.Token);
        }
        catch
        {
        }

        cancellationTokenSource.Cancel();
        await this.webSocketServer.CloseAsync();
    }

    private async Task ProcessResponseData(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var response = await this.subscriptionService.ResultBufferBlock.ReceiveAsync(cancellationToken);
            this.webSocketServer.Send(response);
        }
    }
}

