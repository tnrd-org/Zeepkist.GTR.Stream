using System.Net.WebSockets;
using System.Text;

namespace TNRD.Zeepkist.GTR.Stream;

public class SocketWriter
{
    private readonly SocketRepository repository;

    public SocketWriter(SocketRepository repository)
    {
        this.repository = repository;
    }

    public void Write(string message)
    {
        IReadOnlyList<SocketRepository.SocketData> sockets = repository.GetSockets();
        SendAsync(sockets.ToList(), message);
    }

    private async void SendAsync(IReadOnlyList<SocketRepository.SocketData> sockets, string message)
    {
        foreach (SocketRepository.SocketData socket in sockets)
        {
            try
            {
                await socket.WebSocket.SendAsync(Encoding.UTF8.GetBytes(message),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            catch (Exception e)
            {
                socket.Tcs.SetException(e);
            }
        }
    }
}
