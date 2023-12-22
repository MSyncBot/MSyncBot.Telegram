using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MSyncBot.Telegram.Bot.Handlers.General;

public class UpdateHandler
{
    public Task GetUpdatesAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        switch (update)
        {
            case { Type: UpdateType.Message }:
                return new MessageHandler().MessageHandlerAsync(botClient, update);
        }

        return Task.CompletedTask;
    }
    
}