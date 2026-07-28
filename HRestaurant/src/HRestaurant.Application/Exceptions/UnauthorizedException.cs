namespace HRestaurant.Exceptions;

public sealed class UnauthorizedException : Exception
{
    private const string DefaultMessage =
        "Authentication is required to access this resource.";

    public UnauthorizedException(string message = DefaultMessage)
        : base(message)
    {
    }
}
