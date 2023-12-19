using System.Text.Json;
using MSyncBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using MessageType = Telegram.Bot.Types.Enums.MessageType;

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
                    return;

                case MessageType.Photo:
                case MessageType.Video:
                case MessageType.VideoNote:
                case MessageType.Animation:
                case MessageType.Audio:
                case MessageType.Voice:
                case MessageType.Document:
                    MediaFileHandler.CountingMediaFiles(update.Message);
                    await new MediaFileHandler().GetMediaFilesAsync(botClient, update.Message);
                    return;

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