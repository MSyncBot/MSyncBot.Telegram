using System.Text.Json;
using MSyncBot.Telegram.Bot.Handlers.Server.Types;
using MSyncBot.Telegram.Bot.Handlers.Server.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = System.IO.File;
using Message = MSyncBot.Telegram.Bot.Handlers.Server.Types.Message;
using MessageType = Telegram.Bot.Types.Enums.MessageType;
using User = MSyncBot.Telegram.Bot.Handlers.Server.Types.User;

namespace MSyncBot.Telegram.Bot.Handlers;

public class MessageHandler
{
    
    public async Task MessageHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        try
        {
            var message = update.Message;
            switch (message)
            {
                case { Type: MessageType.Text }:
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
                
                case { Type: MessageType.Photo }:
                {
                    var photoId = update.Message.Photo.Last().FileId;
                    var photoInfo = await botClient.GetFileAsync(photoId);
                    var photoPath = photoInfo.FilePath;

                    var photoName = Guid.NewGuid().ToString();
                    const string extension = ".png";
                    var destinationFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        "UserPhotos",
                        $"{photoName}{extension}");
                    
                    await using (Stream fileStream = File.Create(destinationFilePath))
                    {
                        await botClient.DownloadFileAsync(
                            filePath: photoPath,
                            destination: fileStream);
                    }

                    var photoBytes = await File.ReadAllBytesAsync(destinationFilePath);
                    
                    var mediaFile = new MediaFile(photoName, extension, photoBytes, FileType.Photo);
                    var photoMessage = new Message("MSyncBot.Telegram",
                            1,
                            SenderType.Telegram,
                            Server.Types.Enums.MessageType.Photo,
                            new User(message.From.FirstName));
                    photoMessage.MediaFiles.Add(mediaFile);
                    var jsonPhotoMessage = JsonSerializer.Serialize(photoMessage);
                    Bot.Server.SendTextAsync(jsonPhotoMessage);
                    File.Delete(destinationFilePath);
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