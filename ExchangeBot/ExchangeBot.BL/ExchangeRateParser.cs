using ExchangeBot.BL.Exceptions;
using ExchangeBot.BL.Interfaces;
using ExchangeBot.BL.Models;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace ExchangeBot.BL;

public class ExchangeRateParser : IExchangeRateParser
{
    private const string _url = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange";
    private readonly HttpClient _httpClient = new();
    private readonly Uri _apiUri = new(_url);

    public decimal GetPurchaseRate(string currencyCode, DateOnly date)
    {
        return GetPurchaseRateAsync(currencyCode, date).Result;
    }

    public async Task<decimal> GetPurchaseRateAsync(string currencyCode, DateOnly date)
    {
        var uriBuilder = new UriBuilder(_apiUri);
        uriBuilder.Query += $"?valcode={currencyCode}";
        uriBuilder.Query += $"&date={FormattedDate(date)}";
        uriBuilder.Query += $"&json";
        var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new ResponseFailedException($"Error code: {response.StatusCode}");
        }

        try
        {
            var json = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<ExchangeCurrencyItem>>(json);

            if (list == null || list.Count != 1)
            {
                throw new Exception();
            }

            var item = list.First();
            return item.Rate;
        }
        catch (Exception)
        {
            throw new DataNotFoundExchangeException();
        }
    }

    private string FormattedDate(DateOnly date) => $"{date.Year:0000}{date.Month:00}{date.Day:00}";
}
