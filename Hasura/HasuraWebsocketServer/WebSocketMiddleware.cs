public class WebSocketMiddleware
{
    private readonly RequestDelegate next;
    private readonly WebSocketSessionHandlerFactory webSocketSessionHandlerFactory;

    public WebSocketMiddleware(RequestDelegate next, WebSocketSessionHandlerFactory webSocketSessionHandlerFactory)
    {
        this.next = next;
        this.webSocketSessionHandlerFactory = webSocketSessionHandlerFactory;
    }

    /// <summary>
    /// Invokes the middleware for the specified context.
    /// </summary>
    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path == "/ws")
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var webSocketSessionHandler = this.webSocketSessionHandlerFactory.Create(webSocket);
                await webSocketSessionHandler.ProcessAsync();
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }
        else
        {
            await this.next.Invoke(context);
        }
    }
}

