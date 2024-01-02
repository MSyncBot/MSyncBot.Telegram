using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using MSyncBot.Types.Enums;
using NetCoreServer;
using Telegram.Bot;
using Telegram.Bot.Types;
using Message = MSyncBot.Types.Message;

namespace MSyncBot.Telegram.Bot.Handlers.Server;

public class ServerHandler(string address, int port) : WsClient(address, port)
{
    public void DisconnectAndStop()
    {
        _stop = true;
        CloseAsync(1000);
        while (IsConnected)
            Thread.Yield();
    }

    public override void OnWsConnecting(HttpRequest request)
    {
        request.SetBegin("GET", "/");
        request.SetHeader("Host", "localhost");
        request.SetHeader("Origin", "http://localhost");
        request.SetHeader("Upgrade", "websocket");
        request.SetHeader("Connection", "Upgrade");
        request.SetHeader("Sec-WebSocket-Key", Convert.ToBase64String(WsNonce));
        request.SetHeader("Sec-WebSocket-Protocol", "chat, superchat");
        request.SetHeader("Sec-WebSocket-Version", "13");
        request.SetBody();
    }

    public override void OnWsConnected(HttpResponse response) => Bot.Logger.LogSuccess($"Chat WebSocket client connected a new session with Id {Id}");

    public override void OnWsDisconnected() => Bot.Logger.LogError($"Chat WebSocket client disconnected a session with Id {Id}");

    public override void OnWsReceived(byte[] buffer, long offset, long size) =>
        new ReceivedMessageHandler().ReceiveMessage(buffer, offset, size);

    protected override void OnDisconnected()
    {
        base.OnDisconnected();

        Bot.Logger.LogError($"Chat WebSocket client disconnected a session with Id {Id}");

        Thread.Sleep(1000);

        if (!_stop)
            ConnectAsync();
    }

    protected override void OnError(SocketError error) 
        => Bot.Logger.LogError($"Chat WebSocket client caught an error with code {error}");

    private bool _stop;
}