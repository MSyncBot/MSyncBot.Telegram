using MLoggerService;

namespace MSyncBot.Telegram;

class Program
{
    private static async Task Main()
    {
        var logger = new MLogger();
        
        logger.LogProcess("Initializing database...");
        var database = new MDatabase.
            MDatabase(
                "####",
                "####",
                "####",
                "####");
        
        logger.LogSuccess("Database has been initalized");

        var bot = new Bot.Bot("####", logger, database);
        await bot.StartAsync();
        await Task.Delay(-1);
    }
}