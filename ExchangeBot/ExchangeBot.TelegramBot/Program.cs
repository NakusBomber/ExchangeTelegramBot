using ExchangeBot.BL;
using ExchangeBot.TelegramBot;
using ExchangeBot.TelegramBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

public static partial class Program
{
    private const string pathToFile = "token.json";
    static async Task Main(string[] args)
    {
        var exchanger = new ExchangeCurrency();
        var parser = new ExchangeRateParser();
        var logger = GetLogger();
        var bot = new ExchangeTelegramBot(pathToFile, exchanger, parser, logger);

        Console.ReadKey(true);
        await bot.StopAsync();
    }

    static ILogger GetLogger()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole(options =>
        {
            options.TimestampFormat = "[HH:mm:ss] ";
            options.SingleLine = true;
        }));
        var logger = loggerFactory.CreateLogger("ExchangeBot");
        return logger;
    }
}
