using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using MSyncBot.Telegram.Bot.Handlers.Server.Types.Enums;
using NetCoreServer;
using Telegram.Bot;
using System.Drawing;
using Telegram.Bot.Types;
using File = System.IO.File;
using Message = MSyncBot.Telegram.Bot.Handlers.Server.Types.Message;

namespace MSyncBot.Telegram.Bot.Handlers.Server;

public class Client : WsClient
{
    public Client(string address, int port) : base(address, port) {}

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

    public override void OnWsConnected(HttpResponse response)
    {
        Bot.Logger.LogSuccess($"Chat WebSocket client connected a new session with Id {Id}");
    }

    public override void OnWsDisconnected()
    {
        Bot.Logger.LogError($"Chat WebSocket client disconnected a session with Id {Id}");
    }

    public override void OnWsReceived(byte[] buffer, long offset, long size)
    {
        Task.Run(async () =>
        {
            var jsonMessage = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            var message = JsonSerializer.Deserialize<Message>(jsonMessage);
        
            if (message.SenderType is SenderType.Telegram)
                return;

            var chatId = 823731104;
            switch (message.MessageType)
            {
                case MessageType.Text:
                    Bot.Logger.LogInformation($"Received message: {message.SenderName} - {message.Content}");
                    await Bot.BotClient.SendTextMessageAsync(chatId, 
                        $"{message.Content}\n\n" +
                        $"Время, за которое сообщение пришло: {DateTime.Now - message.Timestamp}");
                    return;
            
                case MessageType.Photo:
                {
                    Bot.Logger.LogInformation($"Received photo: {message.SenderName} - {message.MediaFiles[0].Name}{message.MediaFiles[0].Extension}");

                    var photoBytes = message.MediaFiles[0].Data;
                    using var memoryStream = new MemoryStream(photoBytes);
                    await Bot.BotClient.SendPhotoAsync(
                        chatId,
                        new InputFileStream(memoryStream, $"{message.MediaFiles[0].Name}{message.MediaFiles[0].Extension}"),
                        caption: $"Время, за которое сообщение пришло: {DateTime.Now - message.Timestamp}"
                    );
                    return;
                }

            } 
        });
    }

    protected override void OnDisconnected()
    {
        base.OnDisconnected();

        Bot.Logger.LogError($"Chat WebSocket client disconnected a session with Id {Id}");
        
        Thread.Sleep(1000);
        
        if (!_stop)
            ConnectAsync();
    }

    protected override void OnError(SocketError error)
    {
        Bot.Logger.LogError($"Chat WebSocket client caught an error with code {error}");
    }

    private bool _stop;
}