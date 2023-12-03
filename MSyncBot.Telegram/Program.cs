using MLoggerService;

namespace MSyncBot.Telegram;

class Program
{
    static async Task Main(string[] args)
    {
        var logger = new MLogger();
        
        logger.LogProcess("Initializing database...");
        var database = new MDatabase.
            MDatabase(
                "####",
                "####",
                "####",
                "####");

        //await database.PingAsync();
        logger.LogSuccess("Database has been initalized");

        var bot = new Bot.Bot("####", logger, database);
        await bot.StartAsync();
        await Task.Delay(-1);
    }
}