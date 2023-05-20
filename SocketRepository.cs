using System.Net.WebSockets;

namespace TNRD.Zeepkist.GTR.Stream;

public class SocketRepository
{
    public record SocketData(WebSocket WebSocket, TaskCompletionSource<object> Tcs);

    private readonly List<SocketData> webSockets = new List<SocketData>();

    public void Add(WebSocket webSocket, TaskCompletionSource<object> tcs)
    {
        SocketData socketData = new SocketData(webSocket, tcs);
        webSockets.Add(socketData);
        WaitForSocketClose(socketData);
    }

    private async void WaitForSocketClose(SocketData socketData)
    {
        try
        {
            byte[] buffer = new byte[1024 * 4];
            WebSocketReceiveResult receiveResult = await socketData.WebSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue)
            {
                receiveResult = await socketData.WebSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None);
            }

            await socketData.WebSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }
        catch (Exception e)
        {
            socketData.Tcs.SetException(e);
            return;
        }

        socketData.Tcs.SetCanceled();
    }

    public IReadOnlyList<SocketData> GetSockets()
    {
        return new List<SocketData>(webSockets.Where(x => !x.Tcs.Task.IsCanceled && !x.Tcs.Task.IsFaulted));
    }
}
