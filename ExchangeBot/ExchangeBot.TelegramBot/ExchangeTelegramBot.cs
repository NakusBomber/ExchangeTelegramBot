using ExchangeBot.BL;
using ExchangeBot.BL.Exceptions;
using ExchangeBot.BL.Interfaces;
using ExchangeBot.TelegramBot.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ExchangeBot.TelegramBot;

public class ExchangeTelegramBot
{
	private const string _convertCommand = "/convert [date (dd.MM.yyyy)] [currency_code] [count]";
    private const string _convertExample = "/convert 01.02.2020 USD 2.5";

    private readonly TelegramBotClient _botClient;
	private readonly ILogger? _logger;
	private readonly IExchangeCurrency _exchanger;
	private readonly IExchangeRateParser _parser;
	private readonly CancellationTokenSource _cts = new();

	public ExchangeTelegramBot(
		string pathToToken, 
		IExchangeCurrency exchangeCurrency,
		IExchangeRateParser parser,
		ILogger? logger = null)
	{
		_logger = logger;
		_exchanger = exchangeCurrency;
		_parser = parser;
		
		var botToken = ReadToken(pathToToken);
        Log(LogLevel.Information, "Bot initializing...");
        _botClient = new TelegramBotClient(botToken, cancellationToken: _cts.Token);
		Setup();
        Log(LogLevel.Information, "Bot successfully initialyzed");
    }

	public async Task StopAsync()
	{
		await _botClient.CloseAsync();
	}

	private string ReadToken(string pathToToken)
	{
        try
        {
            var textFile = System.IO.File.ReadAllText(pathToToken);
            var token = JsonConvert.DeserializeObject<BotToken>(textFile);

            if (token == null || string.IsNullOrEmpty(token.Value))
            {
                throw new ArgumentNullException(nameof(token));
            }

            Log(LogLevel.Information, "Bot token successfully readed");
			return token.Value;
        }
        catch (Exception)
        {
            throw new NotFindTokenException();
        }
    }

    private void Setup()
    {
        _botClient.OnMessage += OnGeneralMessage;
        _botClient.OnError += OnError;
    }

    private async Task OnGeneralMessage(Message message, UpdateType type)
    {
        if (string.IsNullOrEmpty(message.Text))
		{
			return;
		}

		if (message.Text.StartsWith('/'))
		{
			await OnCommand(message);
			return;
		}

        Log(LogLevel.Information, $"{message.Chat}: send {type} '{message.Text}'");
    }
    
	private Task OnError(Exception exception, HandleErrorSource source)
    {
        Log(LogLevel.Error, exception.Message);
        return Task.CompletedTask;
    }

    private async Task OnCommand(Message message)
	{
		Log(LogLevel.Information, $"{message.Chat}: send command '{message.Text}'");

		if(message.Text == "/start" || message.Text == "/help")
		{
			await SendHelpMessage(message.Chat);
			return;
        }

		if (message.Text!.StartsWith("/convert"))
		{
			await OnConvertCommand(message);
		}
	}

    private async Task OnConvertCommand(Message message)
    {
		var args = await ParseConvertCommandArgsAsync(message);
		if(args == null)
		{
			return;
		}

		Message responseMessage = await _botClient.SendTextMessageAsync(message.Chat, "Wait please..."); ;
		try
		{
            args.Rate = await _parser.GetPurchaseRateAsync(args.Code, args.Date);
            var result = _exchanger.Purchase(args.Count, args.Rate ?? 0.0M);
			var resultText = $"{args.Date}\n{args.Code} -> UAH\nCount = {args.Count}\nRate = {args.Rate}\nResult = {result}";
            await SendTextIfMessageNotFound(responseMessage, resultText);
        }
		catch (Exception ex)
		{
            if(!(await HandleConvertCommandErrorAsync(message, ex)))
            {
			    Log(LogLevel.Error, ex.Message);
            }
		}
    }

    /// <summary>
    /// Handle error from convert command
    /// </summary>
    /// <param name="message">Receive message</param>
    /// <param name="exception">Exception</param>
    /// <returns>
    /// <see langword="true"/> if exception handled, 
    /// <see langword="false"/> - otherwise
    /// </returns>
	private async Task<bool> HandleConvertCommandErrorAsync(Message message, Exception exception)
    {
        switch (exception)
        {
            case NotValidCurrencyCode:
                await SendTextIfMessageNotFound(message, "Incorrect form currency code");
                return true;
            case ResponseFailedException:
                Log(LogLevel.Warning, exception.Message);
                await SendTextIfMessageNotFound(message, "Error with API :-(");
                return true;
            case DataNotFoundExchangeException:
                await SendTextIfMessageNotFound(message, "Not found data for this date or currency code");
                return true;
            case ArgumentException:
                await SendTextIfMessageNotFound(message, exception.Message);
                return true;
            default:
                return false;
        }
    }

	private async Task<ConvertCommandArgs?> ParseConvertCommandArgsAsync(Message message)
	{
        var chunks = message.Text!.Split();
        if (chunks.Length != 4)
        {
            await SendHelpMessage(message.Chat);
            return null;
        }

        var isSuccess = decimal.TryParse(chunks.ElementAt(3), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal count);
        if (!isSuccess)
        {
            await SendCountArgError(message.Chat);
            return null;
        }

        var code = chunks.ElementAt(2);
        isSuccess = DateOnly.TryParse(chunks.ElementAt(1), out DateOnly date);
        if (!isSuccess)
        {
            await SendDateArgError(message.Chat);
            return null;
        }

		return new ConvertCommandArgs(code, count, date);
    }

	private async Task SendTextIfMessageNotFound(Message message, string text)
	{
		try
		{
            await _botClient.EditMessageTextAsync(message.Chat, message.MessageId, text);
        }
		catch (Exception e)
		{
			Log(LogLevel.Warning, e.Message);
            await _botClient.SendTextMessageAsync(message.Chat, text);
        }
    }

	private async Task SendCountArgError(Chat chat) =>
		await _botClient.SendTextMessageAsync(chat, "Count must be in format: '2.1', '12'");

	private async Task SendDateArgError(Chat chat) =>
		await _botClient.SendTextMessageAsync(chat, "Incorrect format date or error in date");
    
	private async Task SendHelpMessage(Chat chat) =>
        await _botClient.SendTextMessageAsync(
            chat,
            $"Use: \n<code>{_convertCommand}</code>\nExample:\n<code>{_convertExample}</code>",
            parseMode: ParseMode.Html
        );

    private void Log(LogLevel logLevel, string message, params object?[] args) =>
        _logger?.Log(logLevel, message, args);
}
