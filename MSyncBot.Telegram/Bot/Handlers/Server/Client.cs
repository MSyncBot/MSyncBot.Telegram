using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using MSyncBot.Types.Enums;
using NetCoreServer;
using Telegram.Bot;
using Telegram.Bot.Types;
using Message = MSyncBot.Types.Message;

namespace MSyncBot.Telegram.Bot.Handlers.Server;

public class Client : WsClient
{
    public Client(string address, int port) : base(address, port) { }

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

            if (message.Messenger.Type is MessengerType.Telegram)
                return;
            
            var chatId = -1001913731159;
            switch (message.Type)
            {
                case MessageType.Text:
                    Bot.Logger.LogInformation(
                        $"Received message from {message.Messenger.Name}: " +
                        $"{message.User.FirstName} ({message.User.Id}) - {message.Text}");

                    await Bot.BotClient.SendTextMessageAsync(
                        chatId,
                        $"{message.User.FirstName}: {message.Text}"
                    );

                    return;

                case MessageType.Photo:
                    Bot.Logger.LogInformation(
                        $"Received photo from {message.Messenger.Name}: " +
                        $"{message.User.FirstName} ({message.User.Id}) - " +
                        $"{message.Files[0].Name}{message.Files[0].Extension}");

                    var photoBytes = message.Files[0].Data;
                    var photoStream = new MemoryStream(photoBytes);
                    var photo = new InputFileStream(photoStream,
                        $"{message.Files[0].Name}{message.Files[0].Extension}");

                    var photoCaption = string.IsNullOrEmpty(message.Text)
                        ? $"{message.User.FirstName}:"
                        : $"{message.User.FirstName}: {message.Text}";

                    await Bot.BotClient.SendPhotoAsync(chatId,
                        photo,
                        caption: photoCaption);

                    return;

                case MessageType.Video:
                    Bot.Logger.LogInformation(
                        $"Received video from {message.Messenger.Name}: " +
                        $"{message.User.FirstName} ({message.User.Id}) - " +
                        $"{message.Files[0].Name}{message.Files[0].Extension}");

                    var videoBytes = message.Files[0].Data;
                    var videoStream = new MemoryStream(videoBytes);
                    var video = new InputFileStream(videoStream,
                        $"{message.Files[0].Name}{message.Files[0].Extension}");

                    var videoCaption = string.IsNullOrEmpty(message.Text)
                        ? $"{message.User.FirstName}:"
                        : $"{message.User.FirstName}: {message.Text}";

                    await Bot.BotClient.SendVideoAsync(chatId,
                        video,
                        caption: videoCaption);

                    return;

                case MessageType.Audio:
                    Bot.Logger.LogInformation(
                        $"Received audio from {message.Messenger.Name}: " +
                        $"{message.User.FirstName} ({message.User.Id}) - " +
                        $"{message.Files[0].Name}{message.Files[0].Extension}");

                    var audioBytes = message.Files[0].Data;
                    var audioStream = new MemoryStream(audioBytes);
                    var audio = new InputFileStream(audioStream,
                        $"{message.Files[0].Name}{message.Files[0].Extension}");

                    var audioCaption = string.IsNullOrEmpty(message.Text)
                        ? $"{message.User.FirstName}:"
                        : $"{message.User.FirstName}: {message.Text}";

                    await Bot.BotClient.SendAudioAsync(chatId,
                        audio,
                        caption: audioCaption);

                    return;

                case MessageType.Document:
                    Bot.Logger.LogInformation(
                        $"Received document from {message.Messenger.Name}: " +
                        $"{message.User.FirstName} ({message.User.Id}) - " +
                        $"{message.Files[0].Name}{message.Files[0].Extension}");

                    var documentBytes = message.Files[0].Data;
                    var documentStream = new MemoryStream(documentBytes);
                    var document = new InputFileStream(documentStream,
                        $"{message.Files[0].Name}{message.Files[0].Extension}");

                    var documentCaption = string.IsNullOrEmpty(message.Text)
                        ? $"{message.User.FirstName}:"
                        : $"{message.User.FirstName}: {message.Text}";

                    await Bot.BotClient.SendDocumentAsync(chatId,
                        document,
                        caption: documentCaption);

                    return;

                case MessageType.Album:
                {
                    Bot.Logger.LogInformation(
                        $"Received album from {message.Messenger.Name} with {message.Files.Count} files: " +
                        $"{message.User.FirstName} ({message.User.Id})");
                    
                    var files = new List<IAlbumInputMedia>();
                    var isFirstMediaFile = true;
                    foreach (var file in message.Files)
                    {
                        var fileStream = new MemoryStream(file.Data);
                        var inputFile = new InputFileStream(fileStream, $"{file.Name}{file.Extension}");

                        var caption = string.IsNullOrEmpty(message.Text)
                            ? $"{message.User.FirstName}:"
                            : $"{message.User.FirstName}: {message.Text}";
                        switch (file.Type)
                        {
                            case FileType.Photo:
                                var albumPhoto = new InputMediaPhoto(inputFile);
                                if (isFirstMediaFile)
                                    albumPhoto.Caption = caption;
                                files.Add(albumPhoto);
                                break;
                            case FileType.Video:
                                var albumVideo = new InputMediaVideo(inputFile);
                                if (isFirstMediaFile)
                                    albumVideo.Caption = caption;
                                files.Add(albumVideo);
                                break;
                            /*case FileType.Document:
                                var albumDocument = new InputMediaDocument(inputFile);
                                if (isFirstMediaFile)
                                    albumDocument.Caption = caption;
                                Files.Add(albumDocument);
                                break;
                            case FileType.Audio:
                                var albumAudio = new InputMediaAudio(inputFile);
                                if (isFirstMediaFile)
                                    albumAudio.Caption = caption;
                                Files.Add(albumAudio);
                                break;*/
                            default:
                            case FileType.Unknown:
                                break;
                        }

                        isFirstMediaFile = false;
                    }

                    await Bot.BotClient.SendMediaGroupAsync(chatId,
                        files);

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