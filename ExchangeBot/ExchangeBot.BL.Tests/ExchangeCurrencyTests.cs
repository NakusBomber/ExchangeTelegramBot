﻿using ExchangeBot.BL.Exceptions;

namespace ExchangeBot.BL.Tests;

public class ExchangeCurrencyTests
{
    
    [Theory]
    [InlineData(-1, -1)]
    [InlineData(-1.2, 0)]
    [InlineData(0, -1.1)]
    public void Purchase_ArgumentsAreZero(decimal count, decimal rate)
    {
        var exchanger = new ExchangeCurrency();
        Assert.Throws<ArgumentException>(() =>
        {
            exchanger.Purchase(count, rate);
        });
    }

    [Theory]
    [InlineData(10, 1.5, 15)]
    [InlineData(0, 1.5, 0)] 
    [InlineData(10, 0, 0)]  
    [InlineData(5.75, 2, 11.5)] 
    [InlineData(3.33, 3, 9.99)] 
    [InlineData(100, 0.5, 50)] 
    [InlineData(7, 1.2, 8.4)]  
    public void Purchase_Data_Test(decimal count, decimal rate, decimal expected)
    {
        var exchanger = new ExchangeCurrency();
        var actual = exchanger.Purchase(count, rate);
        
        Assert.Equal(expected, actual, 10);
    }
}
