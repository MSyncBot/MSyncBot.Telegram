using System.Net;
using MLoggerService;
using MSyncBot.Telegram.Bot.Handlers.General;
using MSyncBot.Telegram.Bot.Handlers.Server;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace MSyncBot.Telegram.Bot;

public class Bot
{
    public static MLogger? Logger { get; set; }
    public static MDatabase.MDatabase? Database { get; set; }

    public static Client Server { get; set; }
    
    public static ITelegramBotClient BotClient { get; set; }
    
    private string Token { get; }

    public Bot(string token, MLogger logger, MDatabase.MDatabase database)
    {
        Logger = logger;
        Logger.LogProcess("Initializing bot...");
     
        Token = token;
        Database = database;
        Server = new Client("127.0.0.1", 1689);
        
        Logger.LogSuccess("Bot has been initialized.");
    }

    public Task StartAsync()
    {
        Logger.LogProcess("Start receive updates...");
        
        using var cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions()
        {
            ThrowPendingUpdates = true,
            AllowedUpdates = new[]
            {
                UpdateType.Message,
                UpdateType.CallbackQuery,
                UpdateType.ChatMember,
                UpdateType.MyChatMember,
                UpdateType.EditedMessage
            }
        };
        
        BotClient = new TelegramBotClient(Token);
        _ = BotClient.ReceiveAsync(
                new UpdateHandler().GetUpdatesAsync,
                new ErrorHandler().GetApiError,
                receiverOptions, 
                cancellationToken: cts.Token);
        
        Server.ConnectAsync();
        
        Logger.LogSuccess("Bot receiving updates.");
        return Task.CompletedTask;
    }
}