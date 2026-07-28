namespace HRestaurant.Exceptions;

public sealed class ForbiddenException : Exception
{
    private const string DefaultMessage =
        "You do not have permission to access this resource.";

    public ForbiddenException(string message = DefaultMessage)
        : base(message)
    {
    }
}
