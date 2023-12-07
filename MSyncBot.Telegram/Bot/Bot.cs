using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.JavaScript;
using MLoggerService;
using MSyncBot.Telegram.Bot.Handlers;
using MSyncBot.Telegram.Bot.Handlers.General;
using MSyncBot.Telegram.Bot.Handlers.Server;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MSyncBot.Telegram.Bot;

public class Bot
{
    public static MLogger? Logger { get; set; }
    public static MDatabase.MDatabase? Database { get; set; }

    public static ServerHandler Server { get; set; }
    
    private string Token { get; }

    public Bot(string token, MLogger logger, MDatabase.MDatabase database)
    {
        Logger = logger;
        Logger.LogProcess("Initializing bot...");
     
        Token = token;
        Database = database;
        Server = new ServerHandler("127.0.0.1");
        
        Logger.LogSuccess("Bot has been initialized.");
    }

    public async Task StartAsync()
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
        
        var botClient = new TelegramBotClient(Token);
        botClient.ReceiveAsync(
                new UpdateHandler().GetUpdatesAsync,
                new ErrorHandler().GetApiError,
                receiverOptions, 
                cancellationToken: cts.Token);

        _ = Task.Run(async () =>
        {
            await Server.ConnectToServerAsync();
            var stream = Server.TcpClient.GetStream();
            while (Server.TcpClient.Connected)
            {
                var message = await Server.ReceiveMessageAsync(stream);
                Logger.LogInformation(message);
                await botClient.SendTextMessageAsync(823731104, message);
            }
        }, cts.Token);
        
        Logger.LogSuccess("Bot receiving updates.");
    }
}