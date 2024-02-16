using MLoggerService;

namespace MSyncBot.Telegram;

class Program
{
    private static async Task Main()
    {
        var logger = new MLogger();

        var bot = new Bot.Bot("6949999276:AAEKaKjHX9kqTQDiG3j3tpDbIjKsOLJdcoY", logger);
        await bot.StartAsync();
        await Task.Delay(-1);
    }
}