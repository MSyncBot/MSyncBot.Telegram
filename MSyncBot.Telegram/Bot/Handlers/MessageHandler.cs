using MSyncBot.Telegram.Bot.Handlers.Server;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MSyncBot.Telegram.Bot.Handlers;

public class MessageHandler
{
    
    public async Task MessageHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        try
        {
            var message = update.Message;

            Bot.Logger.LogInformation("receive update");
            
            switch (message)
            {
                case { Type: MessageType.Text }:
                    var text = message.Text;
                    if (IsBotCommand(text))
                    {
                        await new CommandsHandler().CommandsHandlerAsync(botClient, update);
                        return;
                    }

                    Bot.Server.SendAsync(message.Text);
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
    
    private bool IsBotCommand(string text) => !string.IsNullOrEmpty(text) && text.StartsWith("/");
}