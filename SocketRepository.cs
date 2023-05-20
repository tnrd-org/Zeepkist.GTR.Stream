using System.Net.WebSockets;

namespace TNRD.Zeepkist.GTR.Stream;

public class SocketRepository
{
    public record SocketData(WebSocket WebSocket, TaskCompletionSource<object> Tcs);

    private readonly List<SocketData> webSockets = new List<SocketData>();

    public void Add(WebSocket webSocket, TaskCompletionSource<object> tcs)
    {
        webSockets.Add(new SocketData(webSocket, tcs));
    }

    public IReadOnlyList<SocketData> GetSockets()
    {
        return new List<SocketData>(webSockets.Where(x => !x.Tcs.Task.IsFaulted));
    }
}
