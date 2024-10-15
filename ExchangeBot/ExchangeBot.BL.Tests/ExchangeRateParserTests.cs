using ExchangeBot.BL.Exceptions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using static System.Net.WebRequestMethods;

namespace ExchangeBot.BL.Tests;

public class ExchangeRateParserTests
{
    private const string _currencyCode = "USD";
    private const string _exampleUrl = "https://example.com";
    private const string _jsonTest = @"[{ ""r030"":840,""txt"":""Äמכאנ ÑØÀ"",""rate"":41.1934,""cc"":""USD"",""exchangedate"":""10.10.2024""}]";
    private readonly DateOnly _date = new DateOnly(2000, 10, 10);

    [Theory]
    [InlineData("U1D")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("US2")]
    [InlineData("U_S")]
    [InlineData("0-a")]
    public async Task GetPurchaseRateAsync_ThrowExceptionOnInvalidCurrencyCode(string code)
    {
        var httpHandler = new HttpMessageHandlerMock(HttpStatusCode.OK, "[]");
        var httpClient = new HttpClient(httpHandler);

        var parser = new ExchangeRateParser(httpClient, _exampleUrl);
        await Assert.ThrowsAsync<NotValidCurrencyCode>(async () =>
        {
            await parser.GetPurchaseRateAsync(code, _date);
        });
    }

    [Fact]
    public async Task GetPurchaseRateAsync_ResponseFailedException()
    {
        var httpHandler = new HttpMessageHandlerMock(HttpStatusCode.BadRequest, "[]");
        var httpClient = new HttpClient(httpHandler);

        var parser = new ExchangeRateParser(httpClient, _exampleUrl);
        await Assert.ThrowsAsync<ResponseFailedException>(async () =>
        {
            await parser.GetPurchaseRateAsync(_currencyCode, _date);
        });
    }

    [Fact]
    public async Task GetPurchaseRateAsync_DataNotFoundExchangeException()
    {
        var httpHandler = new HttpMessageHandlerMock(HttpStatusCode.OK, "[]");
        var httpClient = new HttpClient(httpHandler);

        var parser = new ExchangeRateParser(httpClient, _exampleUrl);
        await Assert.ThrowsAsync<DataNotFoundExchangeException>(async () =>
        {
            await parser.GetPurchaseRateAsync(_currencyCode, _date);
        });
    }

    [Fact]
    public async Task GetPurchaseRateAsync_DataParse()
    {
        var httpHandler = new HttpMessageHandlerMock(HttpStatusCode.OK, _jsonTest);
        var httpClient = new HttpClient(httpHandler);

        var parser = new ExchangeRateParser(httpClient, _exampleUrl);

        var expected = 41.1934M;
        var actual = await parser.GetPurchaseRateAsync(_currencyCode, _date);

        Assert.Equal(expected, actual, 10);
    }
}