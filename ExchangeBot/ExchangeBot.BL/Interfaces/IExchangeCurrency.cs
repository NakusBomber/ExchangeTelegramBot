namespace ExchangeBot.BL.Interfaces;

public interface IExchangeCurrency
{
    public decimal Purchase(decimal count, decimal rate);
}
