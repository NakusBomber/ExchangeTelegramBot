using Newtonsoft.Json;

namespace ExchangeBot.TelegramBot.Models;

public class BotToken
{
    [JsonProperty("token")]
    public string? Value { get; set; }
}
