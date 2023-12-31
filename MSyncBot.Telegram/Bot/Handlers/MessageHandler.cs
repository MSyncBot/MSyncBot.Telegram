using System.Text.Json;
using MSyncBot.Types;
using MSyncBot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Chat = MSyncBot.Types.Chat;
using MessageType = Telegram.Bot.Types.Enums.MessageType;
using User = MSyncBot.Types.User;

namespace MSyncBot.Telegram.Bot.Handlers;

public class MessageHandler
{
    public Task MessageHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        try
        {
            var message = update.Message;
            var user = message.From;
            var chat = message.Chat;
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
                        var textMessage = new Types.Message(
                            new Messenger("MSyncBot.Telegram", MessengerType.Telegram),
                            Types.Enums.MessageType.Text,
                            new User(user.FirstName, (ulong)user.Id)
                            {
                                LastName = user.LastName,
                                Username = user.Username,
                            },
                            new Chat(chat.Title, (ulong)chat.Id))
                        {
                            Text = text
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
                case MessageType.Sticker:
                case MessageType.Document:
                    FileHandler.CountingMediaFiles(message.MediaGroupId);
                    _ = Task.Run(async () =>
                        await new FileHandler().FileHandlerAsync(botClient, message));
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