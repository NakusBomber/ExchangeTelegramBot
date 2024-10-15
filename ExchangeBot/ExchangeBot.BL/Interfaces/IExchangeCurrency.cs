namespace ExchangeBot.BL.Interfaces;

public interface IExchangeCurrency
{
    public string CurrencyCode { get; set; }

    public decimal Purchase(decimal count, decimal rate);
}
