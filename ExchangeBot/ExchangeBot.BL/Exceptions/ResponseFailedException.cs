namespace ExchangeBot.BL.Exceptions;

public class ResponseFailedException : ExchangeException
{
	public ResponseFailedException(string message) : base(message)
	{
	}
}
