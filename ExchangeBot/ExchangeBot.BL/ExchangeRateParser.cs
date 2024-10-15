using ExchangeBot.BL.Exceptions;
using ExchangeBot.BL.Interfaces;
using ExchangeBot.BL.Models;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Web;

namespace ExchangeBot.BL;

public class ExchangeRateParser : IExchangeRateParser
{
    private readonly HttpClient _httpClient;
    private readonly Uri _apiUri;

    public ExchangeRateParser()
        : this(new HttpClient(), "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange")
    {
    }

    public ExchangeRateParser(HttpClient httpClient, string url)
    {
        _httpClient = httpClient;
        if(!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            throw new UriFormatException();
        }
        _apiUri = new Uri(url);
    }

    public decimal GetPurchaseRate(string currencyCode, DateOnly date)
    {
        return GetPurchaseRateAsync(currencyCode, date).Result;
    }

    public async Task<decimal> GetPurchaseRateAsync(string currencyCode, DateOnly date)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, UriWithQueries(currencyCode, date));
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

    private Uri UriWithQueries(string currencyCode, DateOnly date)
    {
        var uriBuilder = new UriBuilder(_apiUri);
        uriBuilder.Query += $"?valcode={currencyCode}";
        uriBuilder.Query += $"&date={FormattedDate(date)}";
        
        uriBuilder.Query += $"&json";
        uriBuilder.Query.TrimEnd('=');
        return uriBuilder.Uri;
    }

    private string FormattedDate(DateOnly date) => $"{date.Year:0000}{date.Month:00}{date.Day:00}";
}
