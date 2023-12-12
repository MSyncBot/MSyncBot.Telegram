using System.Text.Json;
using MSyncBot.Telegram.Bot.Handlers.Server;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Message = MSyncBot.Telegram.Bot.Handlers.Server.Message;

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

                    var multiCastMessage = new Message("MSyncBot.Telegram", 1, SenderType.Telegram, message.Text);
                    var jsonMessage = JsonSerializer.Serialize(multiCastMessage);
                    Bot.Server.SendAsync(jsonMessage);
                    
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