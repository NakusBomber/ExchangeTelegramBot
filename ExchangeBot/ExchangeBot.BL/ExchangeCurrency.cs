using ExchangeBot.BL.Exceptions;
using ExchangeBot.BL.Interfaces;

namespace ExchangeBot.BL;

public class ExchangeCurrency : IExchangeCurrency
{
    public string CurrencyCode { get; set; }

    public ExchangeCurrency(string currencyCode)
    {
        if (!IsValidCurrencyCode(currencyCode))
        {
            throw new NotValidCurrencyCode();
        }
        CurrencyCode = currencyCode.ToUpper();
    }

    public decimal Purchase(decimal count, decimal rate)
    {
        if(count < 0 || rate < 0)
        {
            throw new ArgumentException("Count or rate less than zero");
        }

        return count * rate;
    }

    private bool IsValidCurrencyCode(string currencyCode)
    {
        if (string.IsNullOrEmpty(currencyCode))
        {
            return false;
        }

        if(currencyCode.Any(ch => !char.IsLetter(ch)))
        {
            return false;
        }

        if(currencyCode.Length != 3)
        {
            return false;
        }

        return true;
    }
}
