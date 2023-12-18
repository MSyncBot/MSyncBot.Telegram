using System.Text.Json;
using MSyncBot.Telegram.Bot.Handlers.Server.Types;
using MSyncBot.Telegram.Bot.Handlers.Server.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using MessageType = Telegram.Bot.Types.Enums.MessageType;
using Message = MSyncBot.Telegram.Bot.Handlers.Server.Types.Message;
using User = MSyncBot.Telegram.Bot.Handlers.Server.Types.User;

namespace MSyncBot.Telegram.Bot.Handlers;

public class MessageHandler
{
    public async Task MessageHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        try
        {
            var message = update.Message;
            switch (message.Type)
            {
                case MessageType.Text:
                    var text = message.Text;
                    if (IsBotCommand(text))
                    {
                        await new CommandsHandler().CommandsHandlerAsync(botClient, update);
                        return;
                    }

                    var textMessage = new Message("MSyncBot.Telegram",
                        1,
                        SenderType.Telegram,
                        Server.Types.Enums.MessageType.Text,
                        new User(message.From.FirstName))
                    {
                        Content = text
                    };
                    var jsonTextMessage = JsonSerializer.Serialize(textMessage);
                    Bot.Server.SendTextAsync(jsonTextMessage);
                    return;

                case MessageType.Photo:
                {
                    var photoId = message.Photo.Last().FileId;
                    var photoInfo = await botClient.GetFileAsync(photoId);
                    var photoPath = photoInfo.FilePath;

                    using var photoStream = new MemoryStream();
                    await botClient.DownloadFileAsync(filePath: photoPath, destination: photoStream);
                    photoStream.Seek(0, SeekOrigin.Begin);

                    var photoBytes = photoStream.ToArray();

                    var photoFile = new MediaFile(Guid.NewGuid().ToString(), ".png", photoBytes, FileType.Photo);
                    var photoMessage = new Message("MSyncBot.Telegram",
                        1,
                        SenderType.Telegram,
                        Server.Types.Enums.MessageType.Photo,
                        new User(message.From.FirstName));
                    photoMessage.MediaFiles.Add(photoFile);
                    var jsonPhotoMessage = JsonSerializer.Serialize(photoMessage);
                    Bot.Server.SendTextAsync(jsonPhotoMessage);
                    return;
                }

                case MessageType.Video:
                case MessageType.VideoNote:
                {
                    var videoId = message.Type switch
                    {
                        MessageType.Video => message.Video.FileId,
                        MessageType.VideoNote => message.VideoNote.FileId
                    };
                    
                    var videoInfo = await botClient.GetFileAsync(videoId);
                    var videoPath = videoInfo.FilePath;

                    using var videoStream = new MemoryStream();
                    await botClient.DownloadFileAsync(filePath: videoPath, destination: videoStream);
                    videoStream.Seek(0, SeekOrigin.Begin);

                    var videoBytes = videoStream.ToArray();

                    var videoFile = new MediaFile(Guid.NewGuid().ToString(), ".mp4", videoBytes, FileType.Video);
                    var videoMessage = new Message("MSyncBot.Telegram",
                        1,
                        SenderType.Telegram,
                        Server.Types.Enums.MessageType.Video,
                        new User(message.From.FirstName));
                    videoMessage.MediaFiles.Add(videoFile);
                    var jsonVideoMessage = JsonSerializer.Serialize(videoMessage);
                    Bot.Server.SendTextAsync(jsonVideoMessage);
                    return;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception ex)
        {
            Bot.Logger?.LogError(ex.ToString());
        }
    }

    private static bool IsBotCommand(string text) => !string.IsNullOrEmpty(text) && text.StartsWith("/");
}