namespace HRestaurant.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with identifier '{key}' was not found.")
    {
    }
}
