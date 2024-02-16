using Telegram.Bot;
using Telegram.Bot.Types;

namespace MSyncBot.Telegram.Commands;

public class StartCommand
{
    public async Task SendWelcomeMessageAsync(ITelegramBotClient botClient, Update update)
    {
        try
        {
            var message = update.Message;
            var chat = message.Chat;
            await botClient.SendTextMessageAsync(chat.Id,
                "Hello!");
        }
        catch (Exception ex)
        {
            Bot.Logger.LogError(ex.ToString());
        }
    }
}