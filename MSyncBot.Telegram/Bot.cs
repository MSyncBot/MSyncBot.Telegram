using MLoggerService;
using MSyncBot.Telegram.Handlers.General;
using MSyncBot.Telegram.Handlers.Server;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace MSyncBot.Telegram;

public class Bot
{
    public static MLogger? Logger { get; private set; }
    public static ServerHandler? Server { get; private set; }
    
    public static ITelegramBotClient? BotClient { get; private set; }
    
    private CancellationTokenSource? _cancellationTokenSource;
    
    private string Token { get; }

    public Bot(string token, MLogger logger)
    {
        Logger = logger;
        Logger.LogProcess("Initializing bot...");
     
        Token = token;
        Server = new ServerHandler("127.0.0.1", 1689);
        
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
        
        _cancellationTokenSource = new CancellationTokenSource();
        
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
    
    public async Task StopAsync()
    {
        Logger.LogProcess("Stopping bot...");
        await _cancellationTokenSource.CancelAsync();
        Logger.LogSuccess("Bot stopped");
    }
}