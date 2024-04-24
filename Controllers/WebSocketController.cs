using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;

namespace TNRD.Zeepkist.GTR.Stream.Controllers;

public class WebSocketController : ControllerBase
{
    private readonly SocketRepository repository;
    private readonly ILogger<WebSocketController> logger;

    public WebSocketController(SocketWriter socketWriter, SocketRepository repository,
        ILogger<WebSocketController> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    [Route("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            logger.LogInformation("Incoming connection: {IP}", HttpContext.Connection.RemoteIpAddress);
            
            using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            TaskCompletionSource<object> tcs = new();
            repository.Add(webSocket, tcs);
            try
            {
                await tcs.Task;
            }
            catch (TaskCanceledException)
            {
                // Left empty on purpose
            }
            catch (Exception e)
            {
                // TODO: Log
                logger.LogError(e, "Error occurred while handling WebSocket connection");
            }
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private static async Task Echo(WebSocket webSocket)
    {
        byte[] buffer = new byte[1024 * 4];
        WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer),
            CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            await webSocket.SendAsync(
                new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);

            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None);
        }

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
}
