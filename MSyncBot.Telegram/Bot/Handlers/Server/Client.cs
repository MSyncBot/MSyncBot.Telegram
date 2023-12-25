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

            if (message.SenderType is SenderType.Telegram)
                return;

            var chatId = -1001913731159;
            switch (message.MessageType)
            {
                case MessageType.Text:
                    Bot.Logger.LogInformation(
                        $"Received message from {message.SenderName}: " +
                        $"{message.User.FirstName} ({message.User.Id}) - {message.Content}");

                    await Bot.BotClient.SendTextMessageAsync(
                        chatId,
                        $"{message.User.FirstName}: {message.Content}"
                    );

                    return;

                case MessageType.Photo:
                    Bot.Logger.LogInformation(
                        $"Received photo from {message.SenderName}: " +
                        $"{message.User.FirstName} ({message.User.Id}) - " +
                        $"{message.MediaFiles[0].Name}{message.MediaFiles[0].Extension}");

                    var photoBytes = message.MediaFiles[0].Data;
                    var photoStream = new MemoryStream(photoBytes);
                    var photo = new InputFileStream(photoStream,
                        $"{message.MediaFiles[0].Name}{message.MediaFiles[0].Extension}");

                    var photoCaption = string.IsNullOrEmpty(message.Content)
                        ? $"{message.User.FirstName}:"
                        : $"{message.User.FirstName}: {message.Content}";

                    await Bot.BotClient.SendPhotoAsync(chatId,
                        photo,
                        caption: photoCaption);

                    return;

                case MessageType.Video:
                    Bot.Logger.LogInformation(
                        $"Received video from {message.SenderName}: " +
                        $"{message.User.FirstName} ({message.User.Id}) - " +
                        $"{message.MediaFiles[0].Name}{message.MediaFiles[0].Extension}");

                    var videoBytes = message.MediaFiles[0].Data;
                    var videoStream = new MemoryStream(videoBytes);
                    var video = new InputFileStream(videoStream,
                        $"{message.MediaFiles[0].Name}{message.MediaFiles[0].Extension}");

                    var videoCaption = string.IsNullOrEmpty(message.Content)
                        ? $"{message.User.FirstName}:"
                        : $"{message.User.FirstName}: {message.Content}";

                    await Bot.BotClient.SendVideoAsync(chatId,
                        video,
                        caption: videoCaption);

                    return;

                case MessageType.Audio:
                    Bot.Logger.LogInformation(
                        $"Received audio from {message.SenderName}: " +
                        $"{message.User.FirstName} ({message.User.Id}) - " +
                        $"{message.MediaFiles[0].Name}{message.MediaFiles[0].Extension}");

                    var audioBytes = message.MediaFiles[0].Data;
                    var audioStream = new MemoryStream(audioBytes);
                    var audio = new InputFileStream(audioStream,
                        $"{message.MediaFiles[0].Name}{message.MediaFiles[0].Extension}");

                    var audioCaption = string.IsNullOrEmpty(message.Content)
                        ? $"{message.User.FirstName}:"
                        : $"{message.User.FirstName}: {message.Content}";

                    await Bot.BotClient.SendAudioAsync(chatId,
                        audio,
                        caption: audioCaption);

                    return;

                case MessageType.Document:
                    Bot.Logger.LogInformation(
                        $"Received document from {message.SenderName}: " +
                        $"{message.User.FirstName} ({message.User.Id}) - " +
                        $"{message.MediaFiles[0].Name}{message.MediaFiles[0].Extension}");

                    var documentBytes = message.MediaFiles[0].Data;
                    var documentStream = new MemoryStream(documentBytes);
                    var document = new InputFileStream(documentStream,
                        $"{message.MediaFiles[0].Name}{message.MediaFiles[0].Extension}");

                    var documentCaption = string.IsNullOrEmpty(message.Content)
                        ? $"{message.User.FirstName}:"
                        : $"{message.User.FirstName}: {message.Content}";

                    await Bot.BotClient.SendDocumentAsync(chatId,
                        document,
                        caption: documentCaption);

                    return;

                case MessageType.Album:
                {
                    Bot.Logger.LogInformation(
                        $"Received album from {message.SenderName} with {message.MediaFiles.Count} files: " +
                        $"{message.User.FirstName} ({message.User.Id})");
                    
                    var mediaFiles = new List<IAlbumInputMedia>();
                    var isFirstMediaFile = true;
                    foreach (var file in message.MediaFiles)
                    {
                        var fileStream = new MemoryStream(file.Data);
                        var inputFile = new InputFileStream(fileStream, $"{file.Name}{file.Extension}");

                        var caption = string.IsNullOrEmpty(message.Content)
                            ? $"{message.User.FirstName}:"
                            : $"{message.User.FirstName}: {message.Content}";
                        switch (file.FileType)
                        {
                            case FileType.Photo:
                                var albumPhoto = new InputMediaPhoto(inputFile);
                                if (isFirstMediaFile)
                                    albumPhoto.Caption = caption;
                                mediaFiles.Add(albumPhoto);
                                break;
                            case FileType.Video:
                                var albumVideo = new InputMediaVideo(inputFile);
                                if (isFirstMediaFile)
                                    albumVideo.Caption = caption;
                                mediaFiles.Add(albumVideo);
                                break;
                            /*case FileType.Document:
                                var albumDocument = new InputMediaDocument(inputFile);
                                if (isFirstMediaFile)
                                    albumDocument.Caption = caption;
                                mediaFiles.Add(albumDocument);
                                break;
                            case FileType.Audio:
                                var albumAudio = new InputMediaAudio(inputFile);
                                if (isFirstMediaFile)
                                    albumAudio.Caption = caption;
                                mediaFiles.Add(albumAudio);
                                break;*/
                            default:
                            case FileType.Unknown:
                                break;
                        }

                        isFirstMediaFile = false;
                    }

                    await Bot.BotClient.SendMediaGroupAsync(chatId,
                        mediaFiles);

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