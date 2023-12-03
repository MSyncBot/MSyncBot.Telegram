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
                Task.Run(async () => await new MessageHandler().MessageHandlerAsync(botClient, update),
                    cancellationToken);
                return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
    
}