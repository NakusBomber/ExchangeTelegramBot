using ExchangeBot.BL.Exceptions;
using ExchangeBot.BL.Interfaces;

namespace ExchangeBot.BL;

public class ExchangeCurrency : IExchangeCurrency
{

    public decimal Purchase(decimal count, decimal rate)
    {
        if(count < 0 || rate < 0)
        {
            throw new ArgumentException("Count or rate must more than zero");
        }

        return count * rate;
    }
}
