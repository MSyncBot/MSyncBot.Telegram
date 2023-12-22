using System.Text.Json;
using MSyncBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using MessageType = Telegram.Bot.Types.Enums.MessageType;

namespace MSyncBot.Telegram.Bot.Handlers;

public class MessageHandler
{
    public Task MessageHandlerAsync(ITelegramBotClient botClient, Update update)
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
                        _ = Task.Run(async () => await new CommandsHandler().CommandsHandlerAsync(botClient, update));
                        return Task.CompletedTask;
                    }

                    _ = Task.Run(() =>
                    {
                        var textMessage = new Types.Message("MSyncBot.Telegram",
                            1,
                            SenderType.Telegram,
                            Types.Enums.MessageType.Text,
                            new Types.User(message.From.FirstName))
                        {
                            Content = text
                        };
                        var jsonTextMessage = JsonSerializer.Serialize(textMessage);
                        Bot.Server.SendTextAsync(jsonTextMessage);
                    });
                    return Task.CompletedTask; 

                case MessageType.Photo:
                case MessageType.Video:
                case MessageType.VideoNote:
                case MessageType.Animation:
                case MessageType.Audio:
                case MessageType.Voice:
                case MessageType.Document:
                    MediaFileHandler.CountingMediaFiles(update.Message.MediaGroupId);
                    _ = Task.Run(async () => await new MediaFileHandler().GetMediaFilesAsync(botClient, update.Message));
                    return Task.CompletedTask;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception ex)
        {
            Bot.Logger?.LogError(ex.ToString());
        }

        return Task.CompletedTask;
    }

    private static bool IsBotCommand(string text) => !string.IsNullOrEmpty(text) && text.StartsWith("/");
}