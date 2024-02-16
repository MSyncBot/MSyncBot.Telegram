using MConfiguration;
using MLoggerService;

namespace MSyncBot.Telegram;

internal abstract class Program
{
    private static Task Main()
    {
        var logger = new MLogger();
        logger.LogProcess("Initializing logger...");
        logger.LogSuccess("Logger successfully initialized.");

        logger.LogProcess("Initializing program configuration...");
        var configManager = new ConfigManager();
        var programConfig = new ProgramConfiguration();
        foreach (var property in typeof(ProgramConfiguration).GetProperties())
        {
            var propertyName = property.Name;
            var data = configManager.Get(propertyName);

            if (string.IsNullOrEmpty(data))
            {
                logger.LogInformation($"Enter value for {propertyName}:");
                data = Console.ReadLine();
            }
            
            property.SetValue(programConfig, Convert.ChangeType(data, property.PropertyType));
        }
        
        configManager.Set(programConfig);

        logger.LogSuccess("Program configuration has been initialized.");

        var bot = new Bot(
            programConfig.BotToken, 
            programConfig.ServerIp,
            programConfig.ServerPort,
            logger);
        _ = Task.Run(async () => await bot.StartAsync());

        Thread.Sleep(1500); // waiting for starting bot

        logger.LogInformation("Press any key to close program...");
        Console.ReadKey();
        bot.StopAsync().Wait();
        
        return Task.CompletedTask;
    }
}