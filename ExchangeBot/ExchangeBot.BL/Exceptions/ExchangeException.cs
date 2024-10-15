namespace ExchangeBot.BL.Exceptions;

/// <summary>
/// Basic class for all exchange currency exceptions
/// </summary>
public abstract class ExchangeException : Exception
{
	public ExchangeException()
	{

	}

	public ExchangeException(string message) : base(message)
	{

	}
}
