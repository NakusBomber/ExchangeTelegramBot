using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ExchangeBot.BL.Models;

public class ExchangeCurrencyItem
{
    [JsonProperty("r030")]
    public int CurrencyCodeNumbers { get; set; }

    [JsonProperty("txt")]
    public string? CurrencyCodeText { get; set; }

    [JsonProperty("cc")]
    public string? CurrencyCode {  get; set; }

    [JsonProperty("rate")]
    public decimal Rate { get; set; }
}
