public class WebSocketSessionHandlerFactory
{
    private readonly ILoggerFactory loggerFactory;
    private readonly SubscriptionService subscriptionService;

    public WebSocketSessionHandlerFactory(ILoggerFactory loggerFactory, SubscriptionService subscriptionService)
    {
        this.loggerFactory = loggerFactory;
        this.subscriptionService = subscriptionService;
    }

    public WebSocketSessionHandler Create(System.Net.WebSockets.WebSocket webSocket)
        => new(webSocket, subscriptionService,  this.loggerFactory);
}
