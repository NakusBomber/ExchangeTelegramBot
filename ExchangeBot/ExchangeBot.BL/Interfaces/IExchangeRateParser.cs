namespace ExchangeBot.BL.Interfaces;

public interface IExchangeRateParser
{
    public decimal GetPurchaseRate(string currencyCode, DateOnly date);
    public Task<decimal> GetPurchaseRateAsync(string currencyCode, DateOnly date);
}
