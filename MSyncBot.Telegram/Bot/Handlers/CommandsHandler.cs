using MSyncBot.Telegram.Bot.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MSyncBot.Telegram.Bot.Handlers;

public class CommandsHandler
{
    public async Task CommandsHandlerAsync(ITelegramBotClient botClient, Update update)
    {
        try
        {
            var message = update.Message;
            var command = message.Text;
            var user = message.From;
            
            Bot.Logger.LogInformation(
                $"{user.FirstName} {user.LastName} | {user.Username} ({user.Id}) использовал команду {command}");

            switch (command)
            {
                case "/start":
                    await new StartCommand().SendWelcomeMessageAsync(botClient, update);
                    return;
            }
        }
        catch (Exception ex)
        {
            Bot.Logger.LogError(ex.ToString());
        }
    }
}