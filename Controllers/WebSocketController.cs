using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;

namespace TNRD.Zeepkist.GTR.Stream.Controllers;

public class WebSocketController : ControllerBase
{
    private readonly SocketRepository repository;

    public WebSocketController(SocketWriter socketWriter, SocketRepository repository)
    {
        this.repository = repository;
    }

    [Route("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            repository.Add(webSocket, tcs);
            try
            {
                await tcs.Task;
            }
            catch (TaskCanceledException)
            {
                // Left empty on purpose
            }
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private static async Task Echo(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(
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
