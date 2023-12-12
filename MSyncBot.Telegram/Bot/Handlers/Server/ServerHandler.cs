using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using TcpClient = NetCoreServer.TcpClient;

namespace MSyncBot.Telegram.Bot.Handlers.Server;

public class ServerHandler(IPAddress address, int port) : TcpClient(address, port)
{
    public void DisconnectAndStop()
    {
        _stop = true;
        DisconnectAsync();
        while (IsConnected)
            Thread.Yield();
    }

    protected override void OnConnected()
    {
        Console.WriteLine($"Chat TCP client connected a new session with Id {Id}");
    }

    protected override void OnDisconnected()
    {
        Console.WriteLine($"Chat TCP client disconnected a session with Id {Id}");
        
        Thread.Sleep(1000);
        
        if (!_stop)
            ConnectAsync();
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        var jsonMessage = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
        var receivedMessage = JsonSerializer.Deserialize<Message>(jsonMessage);

        Bot.Logger.LogInformation($"Received message from {receivedMessage.SenderName}: {receivedMessage.Content}");
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Chat TCP client caught an error with code {error}");
    }

    private bool _stop;
}