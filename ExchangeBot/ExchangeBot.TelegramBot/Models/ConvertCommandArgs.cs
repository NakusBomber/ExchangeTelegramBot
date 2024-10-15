namespace ExchangeBot.TelegramBot.Models;

public class ConvertCommandArgs
{
    public string Code { get; set; }
    public decimal Count { get; set; }
    public DateOnly Date {  get; set; }
    public decimal? Rate { get; set; }

    public ConvertCommandArgs(string code, decimal count, DateOnly date)
    {
        Code = code;
        Count = count;
        Date = date;
    }
}
