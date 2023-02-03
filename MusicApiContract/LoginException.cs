namespace MusicParty.MusicApi;

public class LoginException : Exception
{
    public LoginException()
    {
    }

    public LoginException(string message) : base(message)
    {
    }

    public LoginException(string message, Exception inner) : base(message, inner)
    {
    }
}