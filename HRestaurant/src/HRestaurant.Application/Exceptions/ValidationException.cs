using System.Collections.ObjectModel;

namespace HRestaurant.Exceptions;

public sealed class ValidationException : Exception
{
    private const string DefaultMessage =
        "One or more validation errors occurred.";

    public ValidationException(string message = DefaultMessage)
        : base(message)
    {
        Errors = new ReadOnlyDictionary<string, string[]>(
            new Dictionary<string, string[]>());
    }

    public ValidationException(
        IReadOnlyDictionary<string, string[]> errors,
        string message = DefaultMessage)
        : base(message)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var errorCopy = errors.ToDictionary(
            error => error.Key,
            error => error.Value
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            StringComparer.Ordinal);

        Errors = new ReadOnlyDictionary<string, string[]>(errorCopy);
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
